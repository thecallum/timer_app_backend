namespace timer_app.UseCases.Interfaces
{
    public interface IDeleteEventUseCase
    {
        Task<bool> ExecuteAsync(int calendarEventId, int userId);
    }
}
