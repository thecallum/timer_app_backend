using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class UpdateEventUseCase : IUpdateEventUseCase
    {
        private readonly ICalendarEventsGateway _gateway;

        public UpdateEventUseCase(ICalendarEventsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<CalendarEventResponse> ExecuteAsync(int calendarEventId, UpdateEventRequest request, int userId)
        {
            var updatedEvent = await _gateway.UpdateEvent(calendarEventId, request, userId);

            return updatedEvent?.ToResponse();
        }
    }
}
