using System.Net.Http.Json;
using NextBus.Mobile.Models;

namespace NextBus.Mobile.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;

        public GeocodingService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NextBusApp/1.0 (transit-app-student-project)");
        }

        public async Task<List<PlacePrediction>> SearchPlacesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<PlacePrediction>();

            try
            {
                // חיפוש מוגבל לישראל (countrycodes=il) עם תוצאות בפורמט JSON
                var encodedQuery = Uri.EscapeDataString(query);
                var url = $"https://nominatim.openstreetmap.org/search?q={encodedQuery}&format=json&countrycodes=il&addressdetails=1&limit=5";

                var results = await _httpClient.GetFromJsonAsync<List<PlacePrediction>>(url);
                return results ?? new List<PlacePrediction>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geocoding error: {ex.Message}");
                return new List<PlacePrediction>();
            }
        }
    }
}