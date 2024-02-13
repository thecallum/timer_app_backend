using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app.Gateway.Interfaces
{
    public interface ICalendarEventsGateway
    {
        Task<CalendarEvent> CreateEvent(CreateEventRequest request, string userId);
        Task<bool> DeleteEvent(int calendarEventId, string userId);
        Task<IEnumerable<CalendarEvent>> GetAllEvents(string userId, DateTime? startTime, DateTime? endTime);
        Task<CalendarEvent> UpdateEvent(int calendarEventId, UpdateEventRequest request, string userId);
    }
}