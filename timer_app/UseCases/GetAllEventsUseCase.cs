using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class GetAllEventsUseCase : IGetAllEventsUseCase
    {
        private readonly ICalendarEventsGateway _gateway;

        public GetAllEventsUseCase(ICalendarEventsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<IEnumerable<CalendarEventResponse>> ExecuteAsync(GetAllEventsRequest request, string userId)
        {
            var calendarEvents = await _gateway.GetAllEvents(userId, request.StartTime, request.EndTime);

            return calendarEvents.ToResponse();
        }
    }
}
