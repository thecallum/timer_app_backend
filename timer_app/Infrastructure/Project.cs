using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace timer_app.Infrastructure
{
    public class Project
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }  

        public ProjectColor ProjectColor { get; set; }

        public virtual List<CalendarEvent> CalendarEvents { get; set; }
    }
}
