using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MetarTaf.Components.Factories;
using MetarTaf.Components.Models;

namespace MetarTaf.Components.Services
{
    public class MetarService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public MetarService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<Metar?> GetMetarAsync(string icao)
        {
            string url = $"https://avwx.rest/api/metar/{icao}?token={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();



            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<Metar>(responseBody, options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                AirportFactory.ReleaseAirport(icao);
                return null;
            }
        }
    }
}
