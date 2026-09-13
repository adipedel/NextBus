using NextBus.Mobile.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace NextBus.Mobile.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;

        public GeocodingService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<PlacePrediction>> SearchPlacesAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return new List<PlacePrediction>();

            try
            {
                var trimmedQuery = query.Trim();
                var encoded = Uri.EscapeDataString(trimmedQuery);

                // שימוש ב-Photon עם עדיפות גיאוגרפית לאזור המרכז
                var url = $"https://photon.komoot.io/api/?q={encoded}&lat=32.0853&lon=34.7818&limit=8";

                var response = await _httpClient.GetFromJsonAsync<PhotonResponse>(url, cancellationToken);
                if (response?.Features == null || response.Features.Count == 0)
                {
                    // Fallback ל-Nominatim עבור מספרי בתים ספציפיים אם Photon לא מצא
                    return await FallbackNominatimSearchAsync(trimmedQuery, cancellationToken);
                }

                var results = new List<PlacePrediction>();

                foreach (var f in response.Features)
                {
                    if (f.Geometry.Coordinates.Count >= 2)
                    {
                        var lon = f.Geometry.Coordinates[0];
                        var lat = f.Geometry.Coordinates[1];

                        // הרכבת כתובת מלאה: רחוב + מספר בית + עיר
                        string streetPart = !string.IsNullOrWhiteSpace(f.Properties.Street)
                            ? f.Properties.Street
                            : f.Properties.Name;

                        if (!string.IsNullOrWhiteSpace(f.Properties.HouseNumber))
                        {
                            streetPart += $" {f.Properties.HouseNumber}";
                        }

                        var cityPart = !string.IsNullOrWhiteSpace(f.Properties.City)
                            ? f.Properties.City
                            : f.Properties.District;

                        string displayTitle;
                        if (!string.IsNullOrWhiteSpace(streetPart) && !string.IsNullOrWhiteSpace(cityPart))
                        {
                            displayTitle = $"{streetPart}, {cityPart}";
                        }
                        else
                        {
                            displayTitle = !string.IsNullOrWhiteSpace(streetPart) ? streetPart : trimmedQuery;
                        }

                        // מניעת כפילויות של אותה כתובת
                        if (!results.Any(r => r.DisplayName == displayTitle))
                        {
                            results.Add(new PlacePrediction
                            {
                                DisplayName = displayTitle,
                                Latitude = lat,
                                Longitude = lon
                            });
                        }
                    }
                }

                return results;
            }
            catch (OperationCanceledException)
            {
                return new List<PlacePrediction>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Geocoding Error]: {ex.Message}");
                return new List<PlacePrediction>();
            }
        }

        private async Task<List<PlacePrediction>> FallbackNominatimSearchAsync(string query, CancellationToken cancellationToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&countrycodes=il&addressdetails=1&limit=5");

                request.Headers.Add("User-Agent", "NextBusTransitApp/1.0");

                var res = await _httpClient.SendAsync(request, cancellationToken);
                if (!res.IsSuccessStatusCode) return new List<PlacePrediction>();

                var data = await res.Content.ReadFromJsonAsync<List<NominatimItem>>(cancellationToken: cancellationToken);
                if (data == null) return new List<PlacePrediction>();

                return data.Select(d => new PlacePrediction
                {
                    DisplayName = d.DisplayName,
                    Latitude = double.TryParse(d.Lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0,
                    Longitude = double.TryParse(d.Lon, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : 0
                }).ToList();
            }
            catch
            {
                return new List<PlacePrediction>();
            }
        }

        private class NominatimItem
        {
            [JsonPropertyName("display_name")]
            public string DisplayName { get; set; } = string.Empty;

            [JsonPropertyName("lat")]
            public string Lat { get; set; } = string.Empty;

            [JsonPropertyName("lon")]
            public string Lon { get; set; } = string.Empty;
        }
    }
}