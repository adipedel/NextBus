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
            const double delta = 0.012;

            var originCandidates = await _context.Stations
                .Where(s => Math.Abs(s.Latitude - fromLat) <= delta && Math.Abs(s.Longitude - fromLon) <= delta)
                .ToListAsync();

            var destCandidates = await _context.Stations
                .Where(s => Math.Abs(s.Latitude - toLat) <= delta && Math.Abs(s.Longitude - toLon) <= delta)
                .ToListAsync();

            if (!originCandidates.Any() || !destCandidates.Any())
                return Ok(new List<RouteSolution>());

            var originStations = originCandidates
                .Select(s => new { Station = s, Distance = CalculateDistance(fromLat, fromLon, s.Latitude, s.Longitude) })
                .Where(x => x.Distance <= 900)
                .OrderBy(x => x.Distance)
                .Take(15)
                .ToList();

            var destStations = destCandidates
                .Select(s => new { Station = s, Distance = CalculateDistance(toLat, toLon, s.Latitude, s.Longitude) })
                .Where(x => x.Distance <= 900)
                .OrderBy(x => x.Distance)
                .Take(15)
                .ToList();

            if (!originStations.Any() || !destStations.Any())
                return Ok(new List<RouteSolution>());

            var origIds = originStations.Select(o => o.Station.StationId).ToList();
            var destIds = destStations.Select(d => d.Station.StationId).ToList();

            // 1. קווים ישירים (Direct Routes)
            var directMatches = await (from origStop in _context.LineStops
                                       join destStop in _context.LineStops on origStop.LineId equals destStop.LineId
                                       join line in _context.Lines on origStop.LineId equals line.LineId
                                       where origIds.Contains(origStop.StationId)
                                          && destIds.Contains(destStop.StationId)
                                          && origStop.StopSequence < destStop.StopSequence
                                       select new
                                       {
                                           Line = line,
                                           OrigStationId = origStop.StationId,
                                           DestStationId = destStop.StationId,
                                           StopsCount = destStop.StopSequence - origStop.StopSequence
                                       })
                                       .Distinct()
                                       .ToListAsync();

            var solutions = new List<RouteSolution>();

            // כאן מטפלים רק בקווים ישירים באמצעות m
            foreach (var m in directMatches)
            {
                var orig = originStations.First(o => o.Station.StationId == m.OrigStationId);
                var dest = destStations.First(d => d.Station.StationId == m.DestStationId);

                int walkToMins = (int)Math.Ceiling(orig.Distance / 80.0);
                int walkFromMins = (int)Math.Ceiling(dest.Distance / 80.0);
                int rideMins = m.StopsCount * 2 + 2;

                solutions.Add(new RouteSolution
                {
                    LineNumber = m.Line.LineNumber,
                    Company = m.Line.Company,
                    OriginStationName = orig.Station.Name,
                    DestinationStationName = dest.Station.Name,
                    WalkToStationMeters = Math.Round(orig.Distance),
                    WalkToStationMinutes = walkToMins,
                    StopsCount = m.StopsCount,
                    WalkFromStationMeters = Math.Round(dest.Distance),
                    WalkFromStationMinutes = walkFromMins,
                    TotalDurationMinutes = walkToMins + rideMins + walkFromMins,
                    IsDirect = true,
                    IsTransfer = false
                });
            }

            // אם נמצאו קווים ישירים, מחזירים אותם מיד
            if (solutions.Any())
            {
                var topDirect = solutions
                    .GroupBy(s => s.LineNumber)
                    .Select(g => g.OrderBy(s => s.TotalDurationMinutes).First())
                    .OrderBy(s => s.TotalDurationMinutes)
                    .Take(5)
                    .ToList();

                return Ok(topDirect);
            }

            // 2. מסלולים עם החלפה אחת (Transfer)
            var transferMatches = await (from s1 in _context.LineStops
                                         join t1 in _context.LineStops on s1.LineId equals t1.LineId
                                         join t2 in _context.LineStops on t1.StationId equals t2.StationId
                                         join s2 in _context.LineStops on t2.LineId equals s2.LineId
                                         join l1 in _context.Lines on s1.LineId equals l1.LineId
                                         join l2 in _context.Lines on t2.LineId equals l2.LineId
                                         where origIds.Contains(s1.StationId)
                                            && destIds.Contains(s2.StationId)
                                            && s1.StopSequence < t1.StopSequence
                                            && t2.StopSequence < s2.StopSequence
                                            && s1.LineId != t2.LineId
                                         select new
                                         {
                                             Line1 = l1,
                                             Line2 = l2,
                                             OrigStationId = s1.StationId,
                                             TransferStationId = t1.StationId,
                                             DestStationId = s2.StationId,
                                             Stops1 = t1.StopSequence - s1.StopSequence,
                                             Stops2 = s2.StopSequence - t2.StopSequence
                                         })
                                         .Take(3)
                                         .ToListAsync();

            if (transferMatches.Any())
            {
                var transferStationIds = transferMatches.Select(t => t.TransferStationId).Distinct().ToList();
                var transferStations = await _context.Stations
                    .Where(s => transferStationIds.Contains(s.StationId))
                    .ToDictionaryAsync(s => s.StationId, s => s.Name);

                // כאן מטפלים בהחלפות באמצעות t ו-transferName
                foreach (var t in transferMatches)
                {
                    var orig = originStations.First(o => o.Station.StationId == t.OrigStationId);
                    var dest = destStations.First(d => d.Station.StationId == t.DestStationId);
                    string transferName = transferStations.TryGetValue(t.TransferStationId, out var name) ? name : "תחנת מעבר";

                    int walkToMins = (int)Math.Ceiling(orig.Distance / 80.0);
                    int walkFromMins = (int)Math.Ceiling(dest.Distance / 80.0);
                    int rideMins = (t.Stops1 + t.Stops2) * 2 + 6;

                    solutions.Add(new RouteSolution
                    {
                        LineNumber = $"{t.Line1.LineNumber} ➔ {t.Line2.LineNumber}",
                        FirstLineNumber = t.Line1.LineNumber,
                        SecondLineNumber = t.Line2.LineNumber,
                        Company = $"{t.Line1.Company} / {t.Line2.Company}",
                        OriginStationName = orig.Station.Name,
                        TransferStationName = transferName,
                        FirstLegStops = t.Stops1,
                        SecondLegStops = t.Stops2,
                        DestinationStationName = dest.Station.Name,
                        WalkToStationMeters = Math.Round(orig.Distance),
                        WalkToStationMinutes = walkToMins,
                        StopsCount = t.Stops1 + t.Stops2,
                        WalkFromStationMeters = Math.Round(dest.Distance),
                        WalkFromStationMinutes = walkFromMins,
                        TotalDurationMinutes = walkToMins + rideMins + walkFromMins,
                        IsDirect = false,
                        IsTransfer = true
                    });
                }
            }

            return Ok(solutions.OrderBy(s => s.TotalDurationMinutes).ToList());
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double r = 6371e3;
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