using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.Shared.Models;

namespace NextBus.API.Services
{
    public class RealtimeService
    {
        private readonly AppDbContext _context;

        public RealtimeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ArrivalRealTime>> GetArrivalsForStopAsync(string stopCode)
        {
            string cleanCode = stopCode?.Trim() ?? "";

            // 1. איתור התחנה ישירות לפי המחרוזת
            var station = await _context.Stations
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StationCode == cleanCode);

            if (station == null)
            {
                return new List<ArrivalRealTime>();
            }

            // 2. שליפת כל הקווים שעוצרים בתחנה זו
            var lines = await (from ls in _context.LineStops
                               join l in _context.Lines on ls.LineId equals l.LineId
                               where ls.StationId == station.StationId
                               select l)
                               .AsNoTracking()
                               .Distinct()
                               .ToListAsync();

            if (!lines.Any())
            {
                return new List<ArrivalRealTime>();
            }

            var uniqueLines = lines
                .GroupBy(l => l.LineNumber)
                .Select(g => g.First())
                .ToList();

            var results = new List<ArrivalRealTime>();
            var now = DateTime.Now;

            foreach (var line in uniqueLines)
            {
                int.TryParse(line.LineNumber, out int parsedLineNum);

                int intervalMinutes = 12;
                int lineHash = Math.Abs((line.LineNumber + station.StationCode).GetHashCode());
                int lineSalt = lineHash % 8;

                int arrivalInMinutes = (intervalMinutes - (now.Minute % intervalMinutes) + lineSalt) % intervalMinutes;
                if (arrivalInMinutes <= 0) arrivalInMinutes = 3;

                results.Add(new ArrivalRealTime
                {
                    LineId = parsedLineNum > 0 ? parsedLineNum : line.LineId,
                    LineNumber = line.LineNumber,
                    DestinationName = line.Destination,
                    Company = line.Company,
                    MinutesToArrival = arrivalInMinutes,
                    ArrivalText = arrivalInMinutes <= 1 ? "מגיע כעת" : $"בעוד {arrivalInMinutes} דקות",
                    ScheduledTime = now.AddMinutes(arrivalInMinutes),
                    EstimatedTime = now.AddMinutes(arrivalInMinutes),
                    ExpectedArrivalTime = now.AddMinutes(arrivalInMinutes)
                });
            }

            return results.OrderBy(r => r.MinutesToArrival).ToList();
        }
    }
}