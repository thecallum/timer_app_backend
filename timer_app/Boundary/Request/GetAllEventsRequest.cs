namespace timer_app.Boundary.Request
{
    public class GetAllEventsRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
