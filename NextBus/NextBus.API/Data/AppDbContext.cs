using Microsoft.EntityFrameworkCore;
using NextBus.Shared.Models;

namespace NextBus.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Station> Stations { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<LineStop> LineStops { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // אינדקס לשליפה מהירה לפי קוד תחנה
            modelBuilder.Entity<Station>()
                .HasIndex(s => s.StationCode);

            // אינדקסים קריטיים לשליפת קווים לפי תחנה ולהפך
            modelBuilder.Entity<LineStop>()
                .HasIndex(ls => ls.StationId);

            modelBuilder.Entity<LineStop>()
                .HasIndex(ls => ls.LineId);
        }
    }
}