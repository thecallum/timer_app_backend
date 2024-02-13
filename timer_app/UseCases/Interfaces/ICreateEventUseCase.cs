using timer_app.Boundary.Request;
using timer_app.Boundary.Response;

namespace timer_app.UseCases.Interfaces
{
    public interface ICreateEventUseCase
    {
        Task<CalendarEventResponse> ExecuteAsync(CreateEventRequest request, string userId);
    }
}
