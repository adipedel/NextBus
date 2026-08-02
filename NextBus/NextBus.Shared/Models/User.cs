using System;
using System.Collections.Generic;
using System.Text;

namespace NextBus.Shared.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public List<Station> FavoriteStations { get; set; } = new List<Station>();
        public List<Line> FavoriteRoutes { get; set; } = new List<Line>();
    }
}
