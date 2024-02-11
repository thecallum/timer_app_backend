using timer_app.Boundary.Request;
using timer_app.Boundary.Response;

namespace timer_app.UseCases.Interfaces
{
    public interface IUpdateProjectUseCase
    {
        Task<ProjectResponse> ExecuteAsync(int projectId, UpdateProjectRequest request, string userId);
    }
}
