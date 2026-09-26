namespace NextBus.Shared.Models
{
    public class RouteSolution
    {
        public string LineNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;

        // תחנת מוצא
        public string OriginStationName { get; set; } = string.Empty;
        public double OriginStationLat { get; set; }
        public double OriginStationLon { get; set; }
        public double WalkToStationMeters { get; set; }
        public int WalkToStationMinutes { get; set; }

        // תחנת יעד
        public string DestinationStationName { get; set; } = string.Empty;
        public double DestinationStationLat { get; set; }
        public double DestinationStationLon { get; set; }
        public int StopsCount { get; set; }
        public double WalkFromStationMeters { get; set; }
        public int WalkFromStationMinutes { get; set; }
        public int TotalDurationMinutes { get; set; }

        // הגדרה מפורשת של סוג המסלול לתצוגה נוחה
        public bool IsTransfer { get; set; }
        public bool IsDirect { get; set; }

        // פרטי החלפה - אם קיימת
        public string? FirstLineNumber { get; set; }
        public string? SecondLineNumber { get; set; }
        public string? TransferStationName { get; set; }
        public double? TransferStationLat { get; set; }
        public double? TransferStationLon { get; set; }
        public int FirstLegStops { get; set; }
        public int SecondLegStops { get; set; }

        // זמני הגעה של האוטובוסים הקרובים לתחנת העלייה
        public int? NextArrivalMinutes { get; set; }
        public int? FollowingArrivalMinutes { get; set; }

    }


}