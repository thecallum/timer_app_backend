using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.Infrastructure;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class CreateEventUseCase : ICreateEventUseCase
    {
        private readonly ICalendarEventsGateway _gateway;

        public CreateEventUseCase(ICalendarEventsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<CalendarEventResponse> ExecuteAsync(CreateEventRequest request, int userId)
        {
            var createdCalendarEvent = await _gateway.CreateEvent(request, userId);

            return createdCalendarEvent.ToResponse();
        }
    }
}
