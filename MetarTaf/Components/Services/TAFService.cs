using MetarTaf.Components.Models;
using System.Text.Json;

namespace MetarTaf.Components.Services
{
    public class TAFService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TAFService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<TAF?> GetTAFAsync(string icao)
        {
            string url = $"https://avwx.rest/api/taf/{icao}?token={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();



            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<TAF>(responseBody, options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }
    }
}
