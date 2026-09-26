using System;

namespace NextBus.Shared.Models
{
    public class ArrivalRealTime
    {
        public int TripId { get; set; }
        public int LineId { get; set; }
        public int StationId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime EstimatedTime { get; set; }
        public int MinutesToArrival { get; set; }

        public string LineNumber { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string ArrivalText { get; set; } = string.Empty;
        public DateTime ExpectedArrivalTime { get; set; }
    }
}