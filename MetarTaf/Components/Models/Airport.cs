using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MetarTaf.Components.Services;
using MetarTaf.Components.Models;

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

        private Timer? timer;
        private readonly MetarService metarService;
        private readonly TAFService tafService;
        private readonly AirportInfoService airportInfoService;
        private readonly SynchronizationContext? syncContext;
        private readonly string metarStorageFilePath;
        private readonly string tafStorageFilePath;

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

            EnsureDirectoryExists(metarsFolder);
            EnsureDirectoryExists(tafsFolder);

            metarStorageFilePath = Path.Combine(metarsFolder, $"{icao}_metars.json");
            tafStorageFilePath = Path.Combine(tafsFolder, $"{icao}_tafs.json");

            Console.WriteLine($"METAR storage file path: {metarStorageFilePath}");
            Console.WriteLine($"TAF storage file path: {tafStorageFilePath}");

            LoadMetars();
            LoadTafs();
        }

        private void EnsureDirectoryExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine($"Directory created: {folderPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Directory already exists: {folderPath}");
            }
        }

        public async Task InitializeAsync()
        {
            await FetchMetarAsync();
            await FetchTafAsync();
            await FetchAirportInfoAsync();
            timer = new Timer(async _ => await TimerCallback(), null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private async Task TimerCallback()
        {
            Console.WriteLine("TimerCallback invoked");
            await FetchMetarAsync();
            await FetchTafAsync();
        }

        public async Task FetchMetarAsync()
        {
            try
            {
                Console.WriteLine("FetchMetarAsync invoked");
                var metar = await metarService.GetMetarAsync(Icao);
                LastUpdated = DateTime.UtcNow;

                if (metar != null)
                {
                    var metarTime = metar.Time?.Dt ?? DateTime.UtcNow;
                    if (!Metars.ContainsKey(metarTime))
                    {
                        AddMetar(metarTime, metar);
                    }
                }
                SaveMetars();
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx)
            {
                Error = $"Error fetching data: {httpEx.Message}";
                Console.WriteLine(Error);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Error = $"Error initializing data: {ex.Message}";
                Console.WriteLine(Error);
                NotifyStateChanged();
            }
        }

        public async Task FetchTafAsync()
        {
            try
            {
                Console.WriteLine("FetchTafAsync invoked");
                TAF taf = await tafService.GetTAFAsync(Icao);
                LastUpdated = DateTime.UtcNow;

                if (taf != null)
                {
                    var tafTime = taf.time.dt; // Correctly parse the Dt string to DateTime
                    if (!Tafs.ContainsKey((DateTime)tafTime))
                    {
                        AddTaf((DateTime)tafTime, taf);
                    }
                }
                SaveTafs();
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx)
            {
                Error = $"Error fetching TAF data: {httpEx.Message}";
                Console.WriteLine(Error);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Error = $"Error initializing TAF data: {ex.Message}";
                Console.WriteLine(Error);
                NotifyStateChanged();
            }
        }



        public async Task FetchAirportInfoAsync()
        {
            try
            {
                Console.WriteLine("FetchAirportInfoAsync invoked");
                Info = await airportInfoService.GetAirportInfoAsync(Icao);
                NotifyStateChanged();
            }
            catch (HttpRequestException httpEx)
            {
                Error = $"Error fetching airport info: {httpEx.Message}";
                Console.WriteLine(Error);
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Error = $"Error initializing airport info: {ex.Message}";
                Console.WriteLine(Error);
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
                Console.WriteLine("Saving metars to file...");
                var json = JsonSerializer.Serialize(Metars);
                File.WriteAllText(metarStorageFilePath, json);
                Console.WriteLine("Metars saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving metars: {ex.Message}");
            }
        }

        private void SaveTafs()
        {
            try
            {
                Console.WriteLine("Saving TAFs to file...");
                var json = JsonSerializer.Serialize(Tafs);
                File.WriteAllText(tafStorageFilePath, json);
                Console.WriteLine("TAFs saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving TAFs: {ex.Message}");
            }
        }

        private void LoadMetars()
        {
            try
            {
                if (File.Exists(metarStorageFilePath))
                {
                    Console.WriteLine("Loading metars from file...");
                    var json = File.ReadAllText(metarStorageFilePath);
                    var loadedMetars = JsonSerializer.Deserialize<Dictionary<DateTime, Metar>>(json) ?? new Dictionary<DateTime, Metar>();

                    if (loadedMetars.Any())
                    {
                        var lastMetarTime = loadedMetars.Keys.Max();
                        if (DateTime.UtcNow - lastMetarTime > TimeSpan.FromHours(12))
                        {
                            Console.WriteLine("Last metar is older than 12 hours. Clearing metars.");
                            Metars.Clear();
                            File.Delete(metarStorageFilePath);
                        }
                        else
                        {
                            Metars = loadedMetars;
                            Console.WriteLine("Metars loaded successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading metars: {ex.Message}");
                Metars = new Dictionary<DateTime, Metar>();
            }
        }

        private void LoadTafs()
        {
            try
            {
                if (File.Exists(tafStorageFilePath))
                {
                    Console.WriteLine("Loading TAFs from file...");
                    var json = File.ReadAllText(tafStorageFilePath);
                    var loadedTafs = JsonSerializer.Deserialize<Dictionary<DateTime, TAF>>(json) ?? new Dictionary<DateTime, TAF>();

                    if (loadedTafs.Any())
                    {
                        var lastTafTime = loadedTafs.Keys.Max();
                        if (DateTime.UtcNow - lastTafTime > TimeSpan.FromHours(12))
                        {
                            Console.WriteLine("Last TAF is older than 12 hours. Clearing TAFs.");
                            Tafs.Clear();
                            File.Delete(tafStorageFilePath);
                        }
                        else
                        {
                            Tafs = loadedTafs;
                            Console.WriteLine("TAFs loaded successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading TAFs: {ex.Message}");
                Tafs = new Dictionary<DateTime, TAF>();
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
