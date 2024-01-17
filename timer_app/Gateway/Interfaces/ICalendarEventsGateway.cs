using timer_app.Controllers;
using timer_app.Infrastructure;

namespace timer_app.Gateway.Interfaces
{
    public interface ICalendarEventsGateway
    {
        Task<CalendarEvent> CreateEvent(CreateEventRequest request, int userId);
        Task<bool> DeleteEvent(int calendarEventId, int userId);
        Task<IEnumerable<CalendarEvent>> GetAllEvents(DateTime startTime, DateTime endTime, int userId);
        Task<CalendarEvent> UpdateEvent(int calendarEventId, UpdateEventRequest request, int userId);
    }
}