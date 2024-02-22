using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app.Gateway.Interfaces
{
    public interface IProjectGateway
    {
        Task<Project> CreateProject(CreateProjectRequest request, string userId);
        Task<bool> DeleteProject(int projectId, string userId);
        Task<IEnumerable<ProjectWithCount>> GetAllProjects(string userId);
        Task<Project> UpdateProject(int projectId, UpdateProjectRequest request, string userId);
    }
}