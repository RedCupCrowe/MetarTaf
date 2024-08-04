using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MetarTaf.Components.Services;

namespace MetarTaf.Components.Models
{
    public class Airport : IDisposable
    {
        public string Icao { get; set; }
        public Dictionary<DateTime, Metar> Metars { get; set; }
        public DateTime? LastUpdated { get; private set; }
        public string? Error { get; private set; }
        public AirportInfo? Info { get; private set; }

        private Timer? timer;
        private readonly MetarService metarService;
        private readonly AirportInfoService airportInfoService;
        private readonly SynchronizationContext? syncContext;
        private readonly string storageFilePath;

        // Delegate for notifying state changes
        public Action? OnStateChanged { get; set; }

        public Airport(string icao, MetarService metarService, AirportInfoService airportInfoService)
        {
            Icao = icao;
            Metars = new Dictionary<DateTime, Metar>();
            this.metarService = metarService;
            this.airportInfoService = airportInfoService;
            syncContext = SynchronizationContext.Current;

            // Ensure the resources/metars folder exists
            var resourcesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Metars");
            if (!Directory.Exists(resourcesFolder))
            {
                try
                {
                    Directory.CreateDirectory(resourcesFolder);
                    Console.WriteLine($"Directory created: {resourcesFolder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Directory already exists: {resourcesFolder}");
            }

            storageFilePath = Path.Combine(resourcesFolder, $"{icao}_metars.json");
            Console.WriteLine($"Storage file path: {storageFilePath}");
            LoadMetars();
        }

        public async Task InitializeAsync()
        {
            await FetchMetarAsync();
            await FetchAirportInfoAsync();
            timer = new Timer(async _ => await TimerCallback(), null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private async Task TimerCallback()
        {
            Console.WriteLine("TimerCallback invoked");
            await FetchMetarAsync();
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

        private void SaveMetars()
        {
            try
            {
                Console.WriteLine("Saving metars to file...");
                var json = JsonSerializer.Serialize(Metars);
                File.WriteAllText(storageFilePath, json);
                Console.WriteLine("Metars saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving metars: {ex.Message}");
            }
        }

        private void LoadMetars()
        {
            try
            {
                if (File.Exists(storageFilePath))
                {
                    Console.WriteLine("Loading metars from file...");
                    var json = File.ReadAllText(storageFilePath);
                    var loadedMetars = JsonSerializer.Deserialize<Dictionary<DateTime, Metar>>(json) ?? new Dictionary<DateTime, Metar>();

                    if (loadedMetars.Any())
                    {
                        var lastMetarTime = loadedMetars.Keys.Max();
                        if (DateTime.UtcNow - lastMetarTime > TimeSpan.FromHours(12))
                        {
                            Console.WriteLine("Last metar is older than 12 hours. Clearing metars.");
                            Metars.Clear();
                            File.Delete(storageFilePath);
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

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
