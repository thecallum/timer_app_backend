namespace timer_app.Boundary.Request
{

    public class CreateProjectRequest
    {
        public string Description { get; set; }
        public ProjectColorRequest ProjectColor { get; set; }
    }
}
