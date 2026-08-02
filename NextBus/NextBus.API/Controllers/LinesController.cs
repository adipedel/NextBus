using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinesController : ControllerBase
    {
        private static readonly List<Line> MockLines = new List<Line>
        {
            new Line
            {
                LineId = 1,
                LineNumber = "1",
                Company = "דן",
                Origin = "תחנה מרכזית",
                Destination = "עתידים"
            },
            new Line
            {
                LineId = 2,
                LineNumber = "25",
                Company = "קווים",
                Origin = "אוניברסיטה",
                Destination = "רידינג"
            }
        };

        // GET: api/lines
        [HttpGet]
        public ActionResult<IEnumerable<Line>> GetAllLines()
        {
            return Ok(MockLines);
        }

        // GET: api/lines/1
        [HttpGet("{id}")]
        public ActionResult<Line> GetLineById(int id)
        {
            var line = MockLines.FirstOrDefault(l => l.LineId == id);
            if (line == null)
            {
                return NotFound(new { Message = "Line not found" });
            }
            return Ok(line);
        }
    }
}
