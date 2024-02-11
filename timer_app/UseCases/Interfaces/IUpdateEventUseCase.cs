using timer_app.Boundary.Request;
using timer_app.Boundary.Response;

namespace timer_app.UseCases.Interfaces
{
    public interface IUpdateEventUseCase
    {
        Task<CalendarEventResponse> ExecuteAsync(int calendarEventId, UpdateEventRequest request, string userId);
    }
}
