using Microsoft.EntityFrameworkCore;

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
        }
    }
}
