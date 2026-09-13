using Microsoft.EntityFrameworkCore;
using NextBus.Shared.Models;

namespace NextBus.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<LineStop> LineStops { get; set; }

        public DbSet<Station> Stations { get; set; }
        public DbSet<Line> Lines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LineStop>().HasData(
            new LineStop { Id = 1, LineId = 1, StationId = 9, StopSequence = 1 }, // תחנה מרכזית
            new LineStop { Id = 2, LineId = 1, StationId = 5, StopSequence = 2 }, // הבימה
            new LineStop { Id = 3, LineId = 1, StationId = 1, StopSequence = 3 }, // דיזנגוף סנטר
            new LineStop { Id = 4, LineId = 1, StationId = 2, StopSequence = 4 }, // כיכר רבין
            new LineStop { Id = 5, LineId = 1, StationId = 4, StopSequence = 5 }  // רכבת מרכז סבידור
        );
            // זריעת נתונים ראשונית (Seed Data)
            modelBuilder.Entity<Station>().HasData(
            new Station
            {
                StationId = 1,
                Name = "דיזנגוף סנטר / דיזנגוף",
                StationCode = "21543",
                Latitude = 32.0754,
                Longitude = 34.7756
            },
            new Station
            {
                StationId = 2,
                Name = "כיכר רבין / אבן גבירול",
                StationCode = "21588",
                Latitude = 32.0805,
                Longitude = 34.7806
            },
            new Station
            {
                StationId = 3,
                Name = "קניון עזריאלי / דרך מנחם בגין",
                StationCode = "25012",
                Latitude = 32.0745,
                Longitude = 34.7915
            },
            new Station
            {
                StationId = 4,
                Name = "רכבת תל אביב מרכז / סבידור",
                StationCode = "20015",
                Latitude = 32.0838,
                Longitude = 34.7972
            },
            new Station
            {
                StationId = 5,
                Name = "הבימה / שדרות בן ציון",
                StationCode = "21124",
                Latitude = 32.0716,
                Longitude = 34.7788
            },
            new Station
            {
                StationId = 6,
                Name = "לונדון מיניסטור / אבן גבירול",
                StationCode = "21560",
                Latitude = 32.0768,
                Longitude = 34.7812
            },
            new Station
            {
                StationId = 7,
                Name = "נמל תל אביב / הירקון",
                StationCode = "21890",
                Latitude = 32.0965,
                Longitude = 34.7742
            },
            new Station
            {
                StationId = 8,
                Name = "שוק הכרמל / אלנבי",
                StationCode = "21305",
                Latitude = 32.0673,
                Longitude = 34.7695
            },
            new Station
            {
                StationId = 9,
                Name = "תחנה מרכזית חדשה תל אביב",
                StationCode = "26001",
                Latitude = 32.0560,
                Longitude = 34.7794
            },
            new Station
            {
                StationId = 10,
                Name = "אוניברסיטת תל אביב / חיים לבנון",
                StationCode = "23450",
                Latitude = 32.1133,
                Longitude = 34.8044
            }
        );

            modelBuilder.Entity<Line>().HasData(
                new Line { LineId = 1, LineNumber = "5", Company = "דן", Origin = "תחנה מרכזית", Destination = "רכבת צפון" },
                new Line { LineId = 2, LineNumber = "18", Company = "דן", Origin = "בת ים", Destination = "רכבת מרכז" }
            );
        }
    }
}