using System;
using System.Collections.Generic;
using System.Text;

namespace NextBus.Shared.Models
{
    public class RouteSolution
    {
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public double TotalDuration { get; set; }
        public List<Line> Lines { get; set; } = new List<Line>();
    }
}
