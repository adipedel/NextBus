using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LinesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/lines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Line>>> GetAllLines()
        {
            var lines = await _context.Lines.ToListAsync();
            return Ok(lines);
        }

        // GET: api/lines/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Line>> GetLineById(int id)
        {
            var line = await _context.Lines
                .Include(l => l.Stops)
                .FirstOrDefaultAsync(l => l.LineId == id);

            if (line == null)
            {
                return NotFound(new { Message = "Line not found" });
            }

            return Ok(line);
        }
    }
}