using timer_app.Infrastructure;

namespace timer_app.Gateway.Interfaces
{
    public interface IProjectGateway
    {
        Task<Project> CreateProject(Project project, int userId);
        Task<bool> DeleteProject(int projectId, int userId);
        Task<IEnumerable<Project>> GetAllProjects(int userId);
        Task<bool> UpdateProject(Project project, int userId);
    }
}