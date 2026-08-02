using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
