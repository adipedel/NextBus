namespace NextBus.Shared.Models
{
    public class LineStop
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int StationId { get; set; }
        public int StopSequence { get; set; } // סדר התחנה במסלול הקו (1, 2, 3...)
    }
}