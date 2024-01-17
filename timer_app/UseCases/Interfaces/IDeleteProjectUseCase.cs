namespace timer_app.UseCases.Interfaces
{
    public interface IDeleteProjectUseCase
    {
        Task<bool> ExecuteAsync(int projectId, int userId);
    }
}
