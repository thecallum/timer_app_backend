using timer_app.Boundary.Request;
using timer_app.Boundary.Response;

namespace timer_app.UseCases.Interfaces
{
    public interface IGetAllEventsUseCase
    {
        Task<IEnumerable<CalendarEventResponse>> ExecuteAsync(GetAllEventsRequest request, string userId);
    }
}
