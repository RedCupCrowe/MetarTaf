using MetarTaf.Components.Factories;
using MetarTaf.Components.Models;
using MetarTaf.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections;
using System.Text.Json;

namespace MetarTaf.Components.Pages
{
    public partial class OverviewPage
    {
        private DateTime currentTime;
        private Timer? timer;
        private string newIcao = string.Empty;
        private List<Airport> airports = new List<Airport>();
        private bool isInitialized = false;

        [Inject] private IJSRuntime JSRuntime { get; set; }

        private const string AirportsStorageKey = "airports";

        protected override void OnInitialized()
        {
            currentTime = DateTime.UtcNow;
            timer = new Timer(UpdateCurrentTime, null, 0, 1000); // Update every second
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !isInitialized)
            {
                await LoadAirportsFromLocalStorage();
                isInitialized = true;
                StateHasChanged();
            }
        }

        private void UpdateCurrentTime(object? state)
        {
            currentTime = DateTime.UtcNow;
            InvokeAsync(StateHasChanged);
        }

        private async Task AddAirport()
        {
            if (!string.IsNullOrEmpty(newIcao))
            {
                var airport = AirportFactory.GetAirport(newIcao);

                try
                {
                    await airport.InitializeAsync();

                    // Check if the airport has valid data
                    if (airport.Info != null && airport.Metars.Any() && airport.Tafs.Any())
                    {
                        airports.Add(airport);
                        await SaveAirportsToLocalStorage();
                        StateHasChanged();
                    }
                    else
                    {
                        Console.WriteLine($"Invalid airport data for ICAO: {newIcao}");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"Error fetching data for ICAO {newIcao}: {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error for ICAO {newIcao}: {ex.Message}");
                }
            }
        }

        private async Task RemoveAirport(string icao)
        {
            AirportFactory.ReleaseAirport(icao);
        }

        private async Task ClearAllAirports()
        {
            foreach (var airport in airports)
            {
                AirportFactory.ReleaseAirport(airport.Icao);
            }

            airports.Clear(); // Clear the in-memory dictionary
            await SaveAirportsToLocalStorage(); // Update the local storage
            StateHasChanged(); // Notify the UI to re-render
        }

        private async Task SaveAirportsToLocalStorage()
        {
            List<string> icaoList = new List<string>();

            foreach (Airport airport in airports)
            {
                icaoList.Add(airport.Icao);
            }

            await JSRuntime.InvokeVoidAsync("localStorage.setItem", AirportsStorageKey, JsonSerializer.Serialize(icaoList));
        }

        private async Task LoadAirportsFromLocalStorage()
        {
            var icaoListJson = await JSRuntime.InvokeAsync<string>("localStorage.getItem", AirportsStorageKey);
            if (!string.IsNullOrEmpty(icaoListJson))
            {
                var icaoList = JsonSerializer.Deserialize<List<string>>(icaoListJson);
                if (icaoList != null)
                {
                    foreach (var icao in icaoList)
                    {
                        var airport = AirportFactory.GetAirport(icao);
                        airports.Add(airport);
                        await airport.InitializeAsync();
                    }
                }
            }
        }

        private void NavigateToAirportPage(string icao)
        {
            Navigation.NavigateTo($"/Airport/{icao}");
        }

        public void ConfirmReports(Airport airport)
        {
            airport.MarkMetarAsOld();
            airport.MarkTafAsOld();
            StateHasChanged();
        }

        public void ConfirmAllReports()
        {
            foreach (var airport in airports)
            {
                airport.MarkMetarAsOld();
                airport.MarkTafAsOld();
            }
            StateHasChanged();
        }

        public void Dispose()
        {
            timer?.Dispose();
            foreach (var airport in airports)
            {
                AirportFactory.ReleaseAirport(airport.Icao);
            }
        }
    }
}