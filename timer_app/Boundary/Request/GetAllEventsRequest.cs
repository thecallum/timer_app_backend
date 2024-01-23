using Microsoft.AspNetCore.Mvc;

namespace timer_app.Boundary.Request
{
    public class GetAllEventsRequest
    {
        [FromQuery(Name = "startTime")]
        public DateTime? StartTime { get; set; }

        [FromQuery(Name = "endTime")]
        public DateTime? EndTime { get; set; }
    }
}
