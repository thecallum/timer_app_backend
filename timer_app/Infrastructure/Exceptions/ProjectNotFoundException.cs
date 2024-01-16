namespace timer_app.Infrastructure.Exceptions
{
    public class ProjectNotFoundException : Exception
    {
        public readonly int ProjectId;

        public ProjectNotFoundException(int projectId) : base($"Project {projectId} not found.")
        {
            ProjectId = projectId;
        }
    }
}
