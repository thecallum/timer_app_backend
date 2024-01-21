namespace timer_app.Boundary.Request
{
    public class UpdateProjectRequest
    {
        public string Description { get; set; }
        public ProjectColorRequest ProjectColor { get; set; }
    }
}
