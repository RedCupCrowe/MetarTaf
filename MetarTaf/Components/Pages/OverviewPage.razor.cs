using MetarTaf.Components.Models;
using MetarTaf.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace MetarTaf.Components.Pages
{
    public partial class OverviewPage
    {
        private DateTime currentTime;
        private Timer? timer;
        private string newIcao = string.Empty;
        private Dictionary<string, Airport> airports = new();
        private bool isInitialized = false;

        [Inject] private MetarService MetarService { get; set; }
        [Inject] private TAFService tafService { get; set; }
        [Inject] private AirportInfoService AirportInfoService { get; set; }
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
            if (!string.IsNullOrEmpty(newIcao) && !airports.ContainsKey(newIcao))
            {
                var airport = new Airport(newIcao, MetarService, tafService, AirportInfoService);
                airports[newIcao] = airport;
                await airport.InitializeAsync();
                await SaveAirportsToLocalStorage();
                StateHasChanged();
            }
        }

        private async Task RemoveAirport(string icao)
        {
            if (airports.ContainsKey(icao))
            {
                airports[icao].Dispose();
                airports.Remove(icao);
                await SaveAirportsToLocalStorage();
                StateHasChanged();
            }
        }

        private async Task SaveAirportsToLocalStorage()
        {
            var icaoList = airports.Keys.ToList();
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
                        var airport = new Airport(icao, MetarService, tafService, AirportInfoService);
                        airports[icao] = airport;
                        await airport.InitializeAsync();
                    }
                }
            }
        }

        private void NavigateToAirportPage(string icao)
        {
            Navigation.NavigateTo($"/Airport/{icao}");
        }

        public void Dispose()
        {
            timer?.Dispose();
            foreach (var airport in airports.Values)
            {
                airport.Dispose();
            }
        }
    }
}