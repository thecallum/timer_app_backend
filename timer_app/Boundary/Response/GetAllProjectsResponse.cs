namespace timer_app.Boundary.Response
{
    public class GetAllProjectsResponse
    {
        public List<ProjectResponse> Projects { get; set; }
        public int DurationOfEventsWithoutAProject { get; set; }
    }
}
