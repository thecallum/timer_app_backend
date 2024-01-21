using timer_app.Boundary.Response;

namespace timer_app.UseCases.Interfaces
{
    public interface IGetAllProjectsUseCase
    {
        Task<IEnumerable<ProjectResponse>> ExecuteAsync(int userId);
    }
}
