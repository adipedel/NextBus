namespace NextBus.Shared.Models
{
    public class LineStop
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int StationId { get; set; }
        public int StopSequence { get; set; }
    }
}