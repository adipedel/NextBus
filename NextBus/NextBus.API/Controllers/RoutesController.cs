using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        [HttpGet("search")]
        public ActionResult<IEnumerable<RouteSolution>> SearchRoutes([FromQuery] string origin, [FromQuery] string destination)
        {
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                return BadRequest(new { Message = "יש להזין מוצא ויעד תקינים." });
            }

            var mockRoutes = new List<RouteSolution>
            {
                new RouteSolution
                {
                    DepartureTime = DateTime.Now.AddMinutes(5),
                    ArrivalTime = DateTime.Now.AddMinutes(45),
                    TotalDuration = 40.0, 
                    Lines = new List<Line>
                    {
                        new Line
                        {
                            LineId = 1,
                            LineNumber = "1",
                            Company = "דן",
                            Origin = origin,
                            Destination = destination
                        }
                    }
                },
                new RouteSolution
                {
                    DepartureTime = DateTime.Now.AddMinutes(12),
                    ArrivalTime = DateTime.Now.AddMinutes(50),
                    TotalDuration = 38.0,
                    Lines = new List<Line>
                    {
                        new Line
                        {
                            LineId = 2,
                            LineNumber = "25",
                            Company = "קווים",
                            Origin = origin,
                            Destination = destination
                        }
                    }
                }
            };

            return Ok(mockRoutes);
        }
    }
}
