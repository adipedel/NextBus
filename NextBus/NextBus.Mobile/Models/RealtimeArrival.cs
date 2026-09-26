namespace NextBus.Mobile.Models
{
    public class RealtimeArrival
    {
        public string LineNumber { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public int MinutesToArrival { get; set; }
        public bool IsRealtime { get; set; }
        public DateTime ExpectedArrivalTime { get; set; }

        public string ArrivalText => MinutesToArrival <= 1 ? "עכשיו בתחנה" : $"{MinutesToArrival} דק׳";
        public Color ArrivalColor => MinutesToArrival <= 3 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#2196F3");
    }
}