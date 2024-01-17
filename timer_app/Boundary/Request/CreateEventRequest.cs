namespace timer_app.Boundary.Request
{
    public class CreateEventRequest
    {
        public int? ProjectId { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
