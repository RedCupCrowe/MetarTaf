using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MetarTaf.Components.Services;
using MetarTaf.Components.Models;
using MetarTaf.Components.Factories;

namespace MetarTaf.Components.Models
{
    public class Airport : IDisposable
    {
        public string Icao { get; set; }
        public Dictionary<DateTime, Metar> Metars { get; set; }
        public Dictionary<DateTime, TAF> Tafs { get; set; }
        public DateTime? LastUpdated { get; private set; }
        public string? Error { get; private set; }
        public AirportInfo? Info { get; private set; }
        public bool isNewMetar { get; set; }
        public bool isNewTaf { get; set; }

        private Timer? timer;
        private readonly MetarService metarService;
        private readonly TAFService tafService;
        private readonly AirportInfoService airportInfoService;
        private readonly SynchronizationContext? syncContext;
        private readonly string metarStorageFilePath;
        private readonly string tafStorageFilePath;
        private readonly string infoStorageFilePath;
        private int referenceCount;

        // Delegate for notifying state changes
        public Action? OnStateChanged { get; set; }

        public Airport(string icao, MetarService metarService, TAFService tafService, AirportInfoService airportInfoService)
        {
            Icao = icao;
            Metars = new Dictionary<DateTime, Metar>();
            Tafs = new Dictionary<DateTime, TAF>();

            this.metarService = metarService;
            this.tafService = tafService;
            this.airportInfoService = airportInfoService;
            syncContext = SynchronizationContext.Current;

            // Ensure the resources/metars folder exists
            var resourcesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var metarsFolder = Path.Combine(resourcesFolder, "Metars");
            var tafsFolder = Path.Combine(resourcesFolder, "Tafs");
            var infoFolder = Path.Combine(resourcesFolder, "Info");

            EnsureDirectoryExists(metarsFolder);
            EnsureDirectoryExists(tafsFolder);
            EnsureDirectoryExists(infoFolder);

            metarStorageFilePath = Path.Combine(metarsFolder, $"{icao}_metars.json");
            tafStorageFilePath = Path.Combine(tafsFolder, $"{icao}_tafs.json");
            infoStorageFilePath = Path.Combine(infoFolder, $"{icao}_info.json");


            LoadMetars();
            LoadTafs();

            isNewMetar = false;
            isNewTaf = false;
        }

        private void EnsureDirectoryExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine($"[{Icao}] Directory created: {folderPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{Icao}] Error creating directory: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[{Icao}] Directory already exists: {folderPath}");
            }
        }

        public async Task InitializeAsync()
        {
            await FetchAirportInfoAsync();
            await FetchMetarAsync();
            await FetchTafAsync();
            timer = new Timer(async _ => await TimerCallback(), null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private async Task TimerCallback()
        {
            Console.WriteLine($"[{Icao}] TimerCallback invoked");
            await FetchMetarAsync();
            await FetchTafAsync();
        }

        public async Task FetchMetarAsync()
        {
            try
            {
                Console.WriteLine($"[{Icao}] Fetching METAR");

                if (metarService == null)
                {
                    throw new InvalidOperationException("metarService is not initialized.");
                }

                if (string.IsNullOrEmpty(Icao))
                {
                    throw new InvalidOperationException("Icao is not set.");
                }

                var metar = await metarService.GetMetarAsync(Icao);
                LastUpdated = DateTime.UtcNow;

                if (metar != null)
                {
                    var metarTime = metar.Time?.Dt ?? DateTime.UtcNow;
                    if (!Metars.ContainsKey(metarTime))
                    {
                        AddMetar(metarTime, metar);
                        isNewMetar = true;
                    }
                }
                SaveMetars();
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx)
            {
                Error = $"Error fetching METAR data: {httpEx.Message}";
                Console.WriteLine($"[{Icao}] {Error}");
                AirportFactory.ReleaseAirport(Icao);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Error = $"Error initializing METAR data: {ex.Message}";
                Console.WriteLine($"[{Icao}] {Error}");

                NotifyStateChanged();
            }
        }

        public async Task FetchTafAsync()
        {
            try
            {
                Console.WriteLine($"[{Icao}] Fetching TAF");
                if (tafService == null)
                {
                    throw new InvalidOperationException("tafService is not initialized.");
                }
                if (string.IsNullOrEmpty(Icao))
                {
                    throw new ArgumentException("Icao cannot be null or empty");
                }

                TAF taf = await tafService.GetTAFAsync(Icao);
                LastUpdated = DateTime.UtcNow;

                if (taf != null)
                {
                    var tafTime = taf.time.dt; // Correctly parse the Dt string to DateTime
                    if (!Tafs.ContainsKey((DateTime)tafTime))
                    {
                        AddTaf((DateTime)tafTime, taf);
                        isNewTaf = true;
                    }
                }
                SaveTafs();
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx)
            {
                Error = $"Error fetching TAF data: {httpEx.Message}";
                Console.WriteLine($"[{Icao}] {Error}");
                AirportFactory.ReleaseAirport(Icao);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Error = $"Error initializing TAF data: {ex.Message}";
                Console.WriteLine($"[{Icao}] {Error}");
                NotifyStateChanged();
            }
        }

        public async Task FetchAirportInfoAsync()
        {
            try
            {
                Console.WriteLine($"[{Icao}] Fetching AirportInfo");

                if (airportInfoService == null)
                {
                    throw new InvalidOperationException("airportInfoService is not initialized.");
                }

                if (string.IsNullOrEmpty(Icao))
                {
                    throw new InvalidOperationException("Icao is not set.");
                }

                // Check if the info file exists and is not older than one month
                if (File.Exists(infoStorageFilePath))
                {
                    var fileInfo = new FileInfo(infoStorageFilePath);
                    if (fileInfo.LastWriteTime > DateTime.Now.AddMonths(-1))
                    {
                        Info = await LoadAirportInfoAsync();
                        if (Info != null)
                        {
                            NotifyStateChanged();
                            return;
                        }
                    }
                }

                // Fetch from API if not found or outdated
                Info = await airportInfoService.GetAirportInfoAsync(Icao);
                await SaveAirportInfoAsync();
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx)
            {
                Error = $"Error fetching airport info: {httpEx.Message}";
                Console.WriteLine($"[{Icao}] {Error}");
                AirportFactory.ReleaseAirport(Icao);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Error = $"Error initializing airport info: {ex.Message}";
                Console.WriteLine($"[{Icao}] {Error}");
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged()
        {
            if (syncContext != null)
            {
                syncContext.Post(_ => OnStateChanged?.Invoke(), null);
            }
            else
            {
                OnStateChanged?.Invoke();
            }
        }

        public void AddMetar(DateTime time, Metar metar)
        {
            Metars[time] = metar;
        }

        public void AddTaf(DateTime time, TAF taf)
        {
            Tafs[time] = taf;
        }

        private void SaveMetars()
        {
            try
            {
                Console.WriteLine($"[{Icao}] Saving metars to file...");
                var json = JsonSerializer.Serialize(Metars);
                File.WriteAllText(metarStorageFilePath, json);
                Console.WriteLine($"[{Icao}] Metars saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Icao}] Error saving metars: {ex.Message}");
            }
        }

        private void SaveTafs()
        {
            try
            {
                Console.WriteLine($"[{Icao}] Saving TAFs to file...");
                var json = JsonSerializer.Serialize(Tafs);
                File.WriteAllText(tafStorageFilePath, json);
                Console.WriteLine($"[{Icao}] TAFs saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Icao}] Error saving TAFs: {ex.Message}");
            }
        }

        private void LoadMetars()
        {
            try
            {
                if (File.Exists(metarStorageFilePath))
                {
                    Console.WriteLine($"[{Icao}] Loading metars from file...");
                    var json = File.ReadAllText(metarStorageFilePath);
                    var loadedMetars = JsonSerializer.Deserialize<Dictionary<DateTime, Metar>>(json) ?? new Dictionary<DateTime, Metar>();

                    if (loadedMetars.Any())
                    {
                        var now = DateTime.UtcNow;
                        var twelveHoursAgo = now - TimeSpan.FromHours(12);

                        // Remove METARs older than 12 hours
                        var recentMetars = loadedMetars.Where(kvp => kvp.Key >= twelveHoursAgo).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        if (recentMetars.Any())
                        {
                            var lastMetarTime = recentMetars.Keys.Max();
                            if (now - lastMetarTime > TimeSpan.FromHours(12))
                            {
                                Console.WriteLine($"[{Icao}] Last metar is older than 12 hours. Clearing metars.");
                                Metars.Clear();
                                File.Delete(metarStorageFilePath);
                            }
                            else
                            {
                                Metars = recentMetars;
                                Console.WriteLine($"[{Icao}] Metars loaded successfully.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[{Icao}] All metars are older than 12 hours. Clearing metars.");
                            Metars.Clear();
                            File.Delete(metarStorageFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Icao}] Error loading metars: {ex.Message}");
                Metars = new Dictionary<DateTime, Metar>();
            }
        }

        private void LoadTafs()
        {
            try
            {
                if (File.Exists(tafStorageFilePath))
                {
                    Console.WriteLine($"[{Icao}] Loading TAFs from file...");
                    var json = File.ReadAllText(tafStorageFilePath);
                    var loadedTafs = JsonSerializer.Deserialize<Dictionary<DateTime, TAF>>(json) ?? new Dictionary<DateTime, TAF>();

                    if (loadedTafs.Any())
                    {
                        var now = DateTime.UtcNow;
                        var twelveHoursAgo = now - TimeSpan.FromHours(12);

                        // Remove TAFs older than 12 hours
                        var recentTafs = loadedTafs.Where(kvp => kvp.Key >= twelveHoursAgo).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        if (recentTafs.Any())
                        {
                            var lastTafTime = recentTafs.Keys.Max();
                            if (now - lastTafTime > TimeSpan.FromHours(12))
                            {
                                Console.WriteLine($"[{Icao}] Last TAF is older than 12 hours. Clearing TAFs.");
                                Tafs.Clear();
                                File.Delete(tafStorageFilePath);
                            }
                            else
                            {
                                Tafs = recentTafs;
                                Console.WriteLine($"[{Icao}] TAFs loaded successfully.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[{Icao}] All TAFs are older than 12 hours. Clearing TAFs.");
                            Tafs.Clear();
                            File.Delete(tafStorageFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{Icao}] Error loading TAFs: {ex.Message}");
                Tafs = new Dictionary<DateTime, TAF>();
            }
        }


        private async Task SaveAirportInfoAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(Info, options);
            await File.WriteAllTextAsync(infoStorageFilePath, json);
        }

        private async Task<AirportInfo?> LoadAirportInfoAsync()
        {
            if (File.Exists(infoStorageFilePath))
            {
                var json = await File.ReadAllTextAsync(infoStorageFilePath);
                return JsonSerializer.Deserialize<AirportInfo>(json);
            }
            return null;
        }

        public void MarkMetarAsOld()
        {
            isNewMetar = false;
        }

        public void MarkTafAsOld()
        {
            isNewTaf = false;
        }

        public void IncrementReferenceCount()
        {
            referenceCount++;
        }

        // Method to decrement reference count
        public void DecrementReferenceCount()
        {
            referenceCount--;
        }

        // Method to check if the airport is still in use
        public bool IsInUse()
        {
            return referenceCount > 0;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
