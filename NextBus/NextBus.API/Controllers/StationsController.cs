using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Station>>> GetAllStations()
        {
            var stations = await _context.Stations.ToListAsync();
            return Ok(stations);
        }

        [HttpGet("{id}/arrivals")]
        public ActionResult<List<ArrivalRealTime>> GetStationArrivals(int id)
        {
            // נתוני זמן אמת עדיין מחושבים דינמית לפי שעה נוכחית
            var arrivals = new List<ArrivalRealTime>
            {
                new ArrivalRealTime
                {
                    TripId = 101,
                    LineId = 5,
                    StationId = id,
                    ScheduledTime = DateTime.Now.AddMinutes(4),
                    EstimatedTime = DateTime.Now.AddMinutes(3),
                    MinutesToArrival = 3
                },
                new ArrivalRealTime
                {
                    TripId = 102,
                    LineId = 18,
                    StationId = id,
                    ScheduledTime = DateTime.Now.AddMinutes(12),
                    EstimatedTime = DateTime.Now.AddMinutes(10),
                    MinutesToArrival = 10
                }
            };

            return Ok(arrivals);
        }

        [HttpPost]
        public async Task<ActionResult<Station>> CreateStation([FromBody] Station newStation)
        {
            if (newStation == null)
                return BadRequest();

            _context.Stations.Add(newStation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllStations), new { id = newStation.StationId }, newStation);
        }


    }
}