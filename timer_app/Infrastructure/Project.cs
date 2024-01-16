using System.ComponentModel.DataAnnotations;

namespace timer_app.Infrastructure
{
    public class Project
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        //public virtual User User { get; set; }

        public string Description { get; set; }
        public string DisplayColour { get; set; }

        //public virtual List<CalendarEvent> CalendarEvents { get; set; }

    }
}
