namespace NextBus.Shared.Models
{
    public class RouteSolution
    {
        public string LineNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;

        // פרטי עלייה
        public string OriginStationName { get; set; } = string.Empty;
        public double WalkToStationMeters { get; set; }
        public int WalkToStationMinutes { get; set; }

        // פרטי נסיעה
        public string DestinationStationName { get; set; } = string.Empty;
        public int StopsCount { get; set; } // מספר תחנות בדרך

        // פרטי ירידה והגעה
        public double WalkFromStationMeters { get; set; }
        public int WalkFromStationMinutes { get; set; }

        public int TotalDurationMinutes { get; set; }
    }
}