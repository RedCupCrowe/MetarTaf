using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MetarTaf.Components.Factories;
using MetarTaf.Components.Models;

namespace MetarTaf.Components.Services
{
    public class AirportInfoService
    {
        private readonly HttpClient httpClient;

        public AirportInfoService(HttpClient httpClient, string token)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri("https://avwx.rest/api/");
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Token {token}");
        }

        public async Task<AirportInfo?> GetAirportInfoAsync(string ident)
        {
            var response = await httpClient.GetAsync($"station/{ident}?format=json");
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AirportInfo>(responseData);
        }
    }
}
