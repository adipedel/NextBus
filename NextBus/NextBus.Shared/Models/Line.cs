using System;
using System.Collections.Generic;
using System.Text;

namespace NextBus.Shared.Models
{
    public class Line
    {
        public int LineId { get; set; }
        public string LineNumber { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public List<Station> Stops { get; set; } = new List<Station>();
    }
}
