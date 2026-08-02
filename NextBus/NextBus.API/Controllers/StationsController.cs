using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private static readonly List<Station> MockStations = new List<Station>
        {
            new Station
            {
                StationId = 1,
                Name = "תחנת רכבת מרכז",
                StationCode = "21453",
                Latitude = 32.0833,
                Longitude = 34.7958
            },
            new Station
            {
                StationId = 2,
                Name = "קניון עזריאלי",
                StationCode = "21589",
                Latitude = 32.0746,
                Longitude = 34.7920
            }
        };
        
        // GET: api/stations
        [HttpGet]
        public ActionResult<IEnumerable<Station>> GetAllStations()
        {
            return Ok(MockStations);
        }

        // GET: api/stations/1
        [HttpGet("{id}")]
        public ActionResult<Station> GetStationById(int id)
        {
            var station = MockStations.FirstOrDefault(s => s.StationId == id);
            if (station == null)
            {
                return NotFound(new { Message = "Station not found" });
            }
            return Ok(station);
        }

        // GET: api/stations/1/arrivals
        [HttpGet("{id}/arrivals")]
        public ActionResult<IEnumerable<ArrivalRealTime>> GetStationArrivals(int id)
        {
            var arrivals = new List<ArrivalRealTime>
    {
        new ArrivalRealTime
        {
            TripId = 101,
            LineId = 1,
            StationId = id,
            ScheduledTime = DateTime.Now.AddMinutes(5),
            EstimatedTime = DateTime.Now.AddMinutes(4),
            MinutesToArrival = 4
        },
        new ArrivalRealTime
        {
            TripId = 102,
            LineId = 2,
            StationId = id,
            ScheduledTime = DateTime.Now.AddMinutes(12),
            EstimatedTime = DateTime.Now.AddMinutes(15),
            MinutesToArrival = 15
        }
    };

            return Ok(arrivals);
        }
    }
}
