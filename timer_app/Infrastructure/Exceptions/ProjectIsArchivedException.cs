namespace timer_app.Infrastructure.Exceptions
{
    public class ProjectIsArchivedException : Exception
    {
        public readonly int ProjectId;

        public ProjectIsArchivedException(int projectId) : base($"The project {projectId} has been archived.")
        {
            ProjectId = projectId;
        }
    }
}
