using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NextBus.API.Data;
using NextBus.Shared.Models;

namespace NextBus.API.Services
{
    public class GtfsImporterService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GtfsImporterService> _logger;

        // טווח גיאוגרפי: תל אביב וסביבתה
        private const double MinLat = 32.00;
        private const double MaxLat = 32.17;
        private const double MinLon = 34.72;
        private const double MaxLon = 34.87;

        public GtfsImporterService(AppDbContext context, ILogger<GtfsImporterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ImportTelAvivDataAsync(string contentRootPath)
        {
            if (await _context.Stations.AnyAsync() && await _context.LineStops.AnyAsync())
            {
                return;
            }

            string stopsFile = Path.Combine(contentRootPath, "stops.txt");
            string routesFile = Path.Combine(contentRootPath, "routes.txt");
            string tripsFile = Path.Combine(contentRootPath, "trips.txt");
            string stopTimesFile = Path.Combine(contentRootPath, "stop_times.txt");
            string agencyFile = Path.Combine(contentRootPath, "agency.txt");

            if (!File.Exists(stopsFile) || !File.Exists(routesFile) || !File.Exists(tripsFile) || !File.Exists(stopTimesFile))
            {
                _logger.LogError("One or more GTFS files are missing in NextBus.API folder!");
                return;
            }

            // 1. קריאת חברות מפעילות (agency.txt)
            var agencyMap = new Dictionary<string, string>();
            if (File.Exists(agencyFile))
            {
                using var reader = new StreamReader(agencyFile);
                await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length >= 2) agencyMap[parts[0].Trim()] = parts[1].Trim().Trim('"');
                }
            }

            // 2. טעינת תחנות תל אביב מתוך stops.txt
            _logger.LogInformation("Step 1/4: Reading Tel Aviv stations from stops.txt...");
            var stationsToAdd = new List<Station>();
            var stopIdToStation = new Dictionary<string, Station>();

            using (var reader = new StreamReader(stopsFile))
            {
                await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 6) continue;

                    string stopId = parts[0].Trim();
                    string stopCode = parts[1].Trim();
                    string stopName = parts[2].Trim().Trim('"');

                    if (double.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                    {
                        if (lat >= MinLat && lat <= MaxLat && lon >= MinLon && lon <= MaxLon)
                        {
                            var station = new Station
                            {
                                StationCode = stopCode,
                                Name = stopName,
                                Latitude = lat,
                                Longitude = lon
                            };
                            stationsToAdd.Add(station);
                            stopIdToStation[stopId] = station;
                        }
                    }
                }
            }

            await _context.Stations.AddRangeAsync(stationsToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {stationsToAdd.Count} stations.");

            // 3. סריקת stop_times.txt למציאת trip_ids שעוצרים בתחנות אלו
            _logger.LogInformation("Step 2/4: Finding relevant trips in stop_times.txt...");
            var targetStopIds = stopIdToStation.Keys.ToHashSet();
            var relevantTripIds = new HashSet<string>();
            var rawStopTimes = new List<(string TripId, string StopId, int Sequence)>();

            using (var reader = new StreamReader(stopTimesFile))
            {
                await reader.ReadLineAsync();
                int linesRead = 0;

                while (!reader.EndOfStream)
                {
                    linesRead++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 5) continue;

                    string tripId = parts[0].Trim();
                    string stopId = parts[3].Trim();

                    if (targetStopIds.Contains(stopId))
                    {
                        relevantTripIds.Add(tripId);
                        int.TryParse(parts[4], out int seq);
                        rawStopTimes.Add((tripId, stopId, seq));
                    }
                }
            }

            // 4. מיפוי Trip -> Route מתוך trips.txt
            _logger.LogInformation("Step 3/4: Mapping trips to routes from trips.txt...");
            var tripToRouteMap = new Dictionary<string, string>();
            var relevantRouteIds = new HashSet<string>();

            using (var reader = new StreamReader(tripsFile))
            {
                await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 3) continue;

                    string routeId = parts[0].Trim();
                    string tripId = parts[2].Trim();

                    if (relevantTripIds.Contains(tripId))
                    {
                        tripToRouteMap[tripId] = routeId;
                        relevantRouteIds.Add(routeId);
                    }
                }
            }

            // 5. טעינת הקווים האמיתיים מתוך routes.txt
            _logger.LogInformation("Step 4/4: Loading real line definitions from routes.txt...");
            var routeIdToEntity = new Dictionary<string, Line>();
            var linesToAdd = new List<Line>();

            using (var reader = new StreamReader(routesFile))
            {
                await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 4) continue;

                    string routeId = parts[0].Trim();
                    if (relevantRouteIds.Contains(routeId))
                    {
                        string agencyId = parts[1].Trim();
                        string lineNumber = parts[2].Trim().Trim('"');
                        string longName = parts[3].Trim().Trim('"');

                        string company = agencyMap.TryGetValue(agencyId, out var comp) ? comp : "תחבורה ציבורית";
                        string origin = "מוצא";
                        string destination = longName;

                        if (longName.Contains("<->"))
                        {
                            var split = longName.Split("<->");
                            origin = split[0].Trim();
                            destination = split[1].Trim();
                        }

                        var lineEntity = new Line
                        {
                            LineNumber = lineNumber,
                            Company = company,
                            Origin = origin,
                            Destination = destination
                        };

                        linesToAdd.Add(lineEntity);
                        routeIdToEntity[routeId] = lineEntity;
                    }
                }
            }

            await _context.Lines.AddRangeAsync(linesToAdd);
            await _context.SaveChangesAsync();

            // 6. שמירת הקישורים ב-LineStops
            var lineStopsToAdd = new List<LineStop>();
            var seenLinks = new HashSet<string>();

            foreach (var item in rawStopTimes)
            {
                if (tripToRouteMap.TryGetValue(item.TripId, out var routeId) &&
                    routeIdToEntity.TryGetValue(routeId, out var lineEntity) &&
                    stopIdToStation.TryGetValue(item.StopId, out var stationEntity))
                {
                    string linkKey = $"{lineEntity.LineId}_{stationEntity.StationId}";
                    if (!seenLinks.Contains(linkKey))
                    {
                        seenLinks.Add(linkKey);
                        lineStopsToAdd.Add(new LineStop
                        {
                            LineId = lineEntity.LineId,
                            StationId = stationEntity.StationId,
                            StopSequence = item.Sequence
                        });
                    }
                }
            }

            await _context.LineStops.AddRangeAsync(lineStopsToAdd);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Done! Linked {lineStopsToAdd.Count} real line-stop relationships purely from GTFS files.");
        }
    }
}