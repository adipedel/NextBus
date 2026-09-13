using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.Shared.Models;

namespace NextBus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("plan")]
        public async Task<ActionResult<List<RouteSolution>>> PlanRoute(
            [FromQuery] double fromLat,
            [FromQuery] double fromLon,
            [FromQuery] double toLat,
            [FromQuery] double toLon)
        {
            var stations = await _context.Stations.ToListAsync();
            var lineStops = await _context.LineStops.ToListAsync();
            var lines = await _context.Lines.ToListAsync();

            if (!stations.Any() || !lineStops.Any())
                return Ok(new List<RouteSolution>());

            // מציאת תחנות קרובות למוצא (עד 1.5 ק"מ)
            var originStations = stations
                .Select(s => new { Station = s, Distance = CalculateDistance(fromLat, fromLon, s.Latitude, s.Longitude) })
                .Where(x => x.Distance <= 1500)
                .OrderBy(x => x.Distance)
                .ToList();

            // מציאת תחנות קרובות ליעד (עד 1.5 ק"מ)
            var destStations = stations
                .Select(s => new { Station = s, Distance = CalculateDistance(toLat, toLon, s.Latitude, s.Longitude) })
                .Where(x => x.Distance <= 1500)
                .OrderBy(x => x.Distance)
                .ToList();

            var solutions = new List<RouteSolution>();

            // חיפוש קו שמחבר בין אחת מתחנות המוצא לאחת מתחנות היעד
            foreach (var orig in originStations)
            {
                var origStops = lineStops.Where(ls => ls.StationId == orig.Station.StationId);

                foreach (var origStop in origStops)
                {
                    foreach (var dest in destStations)
                    {
                        var matchingDestStop = lineStops.FirstOrDefault(ls =>
                            ls.LineId == origStop.LineId &&
                            ls.StationId == dest.Station.StationId &&
                            ls.StopSequence > origStop.StopSequence); // מוודא שהנסיעה בכיוון הנכון

                        if (matchingDestStop != null)
                        {
                            var line = lines.FirstOrDefault(l => l.LineId == origStop.LineId);
                            int stopsCount = matchingDestStop.StopSequence - origStop.StopSequence;

                            // הערכת זמני הליכה: ~80 מטר לדקה (מהירות הליכה ממוצעת של כ-4.8 קמ"ש)
                            int walkToMins = (int)Math.Ceiling(orig.Distance / 80.0);
                            int walkFromMins = (int)Math.Ceiling(dest.Distance / 80.0);
                            int rideMins = stopsCount * 3; // הערכה של 3 דקות בין תחנות

                            solutions.Add(new RouteSolution
                            {
                                LineNumber = line?.LineNumber ?? "",
                                Company = line?.Company ?? "",
                                OriginStationName = orig.Station.Name,
                                WalkToStationMeters = Math.Round(orig.Distance),
                                WalkToStationMinutes = walkToMins,
                                DestinationStationName = dest.Station.Name,
                                StopsCount = stopsCount,
                                WalkFromStationMeters = Math.Round(dest.Distance),
                                WalkFromStationMinutes = walkFromMins,
                                TotalDurationMinutes = walkToMins + rideMins + walkFromMins
                            });
                        }
                    }
                }
            }

            // מיון לפי משך הנסיעה הכולל הקצר ביותר
            return Ok(solutions.OrderBy(s => s.TotalDurationMinutes).ToList());
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double r = 6371e3; // רדיוס כדור הארץ במטרים
            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double deltaPhi = (lat2 - lat1) * Math.PI / 180;
            double deltaLambda = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return r * c;
        }
    }
}