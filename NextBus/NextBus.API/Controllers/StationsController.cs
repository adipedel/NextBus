using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.API.Services;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RealtimeService _realtimeService;

        public StationsController(AppDbContext context, RealtimeService realtimeService)
        {
            _context = context;
            _realtimeService = realtimeService;
        }

        // שליפת כל התחנות
        [HttpGet]
        public async Task<ActionResult<List<Station>>> GetStations()
        {
            var stations = await _context.Stations.ToListAsync();
            return Ok(stations);
        }

        // שליפת תחנה בודדת לפי מזהה
        [HttpGet("{id}")]
        public async Task<ActionResult<Station>> GetStation(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            return Ok(station);
        }

        // שליפת זמני הגעה בזמן אמת לפי קוד תחנה (Stop Code)
        [HttpGet("{stopCode}/realtime")]
        public async Task<ActionResult<List<ArrivalRealTime>>> GetRealtime(string stopCode)
        {
            var arrivals = await _realtimeService.GetArrivalsForStopAsync(stopCode);
            return Ok(arrivals);
        }
    }
}