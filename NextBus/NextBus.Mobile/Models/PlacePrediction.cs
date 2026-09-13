using System.Text.Json.Serialization;

namespace NextBus.Mobile.Models
{
    public class PhotonResponse
    {
        [JsonPropertyName("features")]
        public List<PhotonFeature> Features { get; set; } = new();
    }

    public class PhotonFeature
    {
        [JsonPropertyName("properties")]
        public PhotonProperties Properties { get; set; } = new();

        [JsonPropertyName("geometry")]
        public PhotonGeometry Geometry { get; set; } = new();
    }

    public class PhotonProperties
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;

        [JsonPropertyName("housenumber")]
        public string HouseNumber { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("district")]
        public string District { get; set; } = string.Empty;
    }

    public class PhotonGeometry
    {
        // ב-GeoJSON הקואורדינטות מגיעות בסדר [Longitude, Latitude]
        [JsonPropertyName("coordinates")]
        public List<double> Coordinates { get; set; } = new();
    }

    public class PlacePrediction
    {
        public string DisplayName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}