namespace NextBus.Shared.Models
{
    public class RouteSolution
    {
        public string LineNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string OriginStationName { get; set; } = string.Empty;
        public double WalkToStationMeters { get; set; }
        public int WalkToStationMinutes { get; set; }

        public string DestinationStationName { get; set; } = string.Empty;
        public int StopsCount { get; set; }
        public double WalkFromStationMeters { get; set; }
        public int WalkFromStationMinutes { get; set; }
        public int TotalDurationMinutes { get; set; }

        // הגדרה מפורשת של סוג המסלול לתצוגה נוחה ב-XAML
        public bool IsTransfer { get; set; }
        public bool IsDirect { get; set; }

        public string? FirstLineNumber { get; set; }
        public string? SecondLineNumber { get; set; }
        public string? TransferStationName { get; set; }
        public int FirstLegStops { get; set; }
        public int SecondLegStops { get; set; }
    }
}