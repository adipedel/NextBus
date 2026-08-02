namespace NextBus.Shared.Models
{
    public class Station
    {
        public int StationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<Line> PassingLines { get; set; } = new List<Line>();
    }
}
