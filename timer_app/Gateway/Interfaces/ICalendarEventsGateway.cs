using timer_app.Infrastructure;

namespace timer_app.Gateway.Interfaces
{
    public interface ICalendarEventsGateway
    {
        Task<CalendarEvent> CreateEvent(CalendarEvent calendarEvent, int userId);
        Task<bool> DeleteEvent(int calendarEventId, int userId);
        Task<IEnumerable<CalendarEvent>> GetAllEvents(DateTime startTime, DateTime endTime, int userId);
        Task<bool> UpdateEvent(CalendarEvent calendarEvent, int userId);
    }
}