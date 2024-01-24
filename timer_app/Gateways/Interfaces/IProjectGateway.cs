using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app.Gateway.Interfaces
{
    public interface IProjectGateway
    {
        Task<Project> CreateProject(CreateProjectRequest request, int userId);
        Task<bool> DeleteProject(int projectId, int userId);
        Task<IEnumerable<ProjectWithCount>> GetAllProjects(int userId);
        Task<Project> UpdateProject(int projectId, UpdateProjectRequest request, int userId);
    }
}