using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace timer_app.Infrastructure
{
    public class TimerAppDbContext : DbContext
    {
        public TimerAppDbContext(DbContextOptions<TimerAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().OwnsOne(p => p.ProjectColor);


            modelBuilder.Entity<CalendarEvent>()
            .HasIndex(b => new { b.UserId, b.StartTime, b.EndTime })
            .HasDatabaseName("IX_Event_Start_End");
        }
    }
}
