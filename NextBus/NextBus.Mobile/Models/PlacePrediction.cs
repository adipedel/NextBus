using System.Text.Json.Serialization;

namespace NextBus.Mobile.Models
{
    public class PlacePrediction
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public string LatStr { get; set; } = string.Empty;

        [JsonPropertyName("lon")]
        public string LonStr { get; set; } = string.Empty;

        public double Latitude => double.TryParse(LatStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 0;
        public double Longitude => double.TryParse(LonStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : 0;
    }
}