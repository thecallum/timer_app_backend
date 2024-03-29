﻿namespace timer_app.Boundary.Response
{
    public class CalendarEventResponse
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
