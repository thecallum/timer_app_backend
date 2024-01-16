using System.ComponentModel.DataAnnotations;

namespace timer_app.Infrastructure
{
    public class CalendarEvent
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }

        public int? ProjectId { get; set; }
        public virtual Project Project { get; set; }


        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
