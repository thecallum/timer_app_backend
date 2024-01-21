using timer_app.Boundary.Request;
using timer_app.Boundary.Response;

namespace timer_app.UseCases.Interfaces
{
    public interface ICreateProjectUseCase
    {
        Task<ProjectResponse> ExecuteAsync(CreateProjectRequest request, int userId);
    }
}
