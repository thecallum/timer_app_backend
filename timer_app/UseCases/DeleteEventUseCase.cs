using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class DeleteEventUseCase : IDeleteEventUseCase
    {
        private readonly ICalendarEventsGateway _gateway;

        public DeleteEventUseCase(ICalendarEventsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<bool> ExecuteAsync(int calendarEventId, int userId)
        {
            return await _gateway.DeleteEvent(calendarEventId, userId);
        }
    }
}
