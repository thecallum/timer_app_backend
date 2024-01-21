using Microsoft.AspNetCore.Mvc;

namespace timer_app.Boundary.Request
{
    public class EventQuery
    {
        [FromRoute(Name = "eventId")]
        public int EventId { get; set; }
    }
}
