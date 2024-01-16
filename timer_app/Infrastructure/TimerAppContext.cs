using Microsoft.EntityFrameworkCore;

namespace timer_app.Infrastructure
{
    public class TimerAppContext : DbContext
    {
        public TimerAppContext(DbContextOptions<TimerAppContext> options)
            : base(options)
        {
        }

        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}
