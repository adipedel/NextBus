namespace NextBus.API.Models
{
    public class RealtimeArrivalDto
    {
        public string LineNumber { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int MinutesToArrival { get; set; }
        public bool IsRealtime { get; set; } = true;
        public DateTime ExpectedArrivalTime { get; set; }
    }
}