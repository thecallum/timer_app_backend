using Microsoft.EntityFrameworkCore;
using timer_app.Gateway.Interfaces;
using timer_app.Infrastructure;
using timer_app.Infrastructure.Exceptions;

namespace timer_app.Gateway
{
    public class CalendarEventsGateway : ICalendarEventsGateway
    {
        private readonly TimerAppContext _context;


        public CalendarEventsGateway(TimerAppContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CalendarEvent>> GetAllEvents(DateTime startTime, DateTime endTime, int userId)
        {
            var calendarEvents = await _context.CalendarEvents
                .Include(x => x.Project)
                .Where(x => x.UserId == userId)
                .Where(x => x.StartTime >= startTime && x.EndTime < endTime)
                .ToListAsync();

            return calendarEvents;
        }

        public async Task<CalendarEvent> CreateEvent(CalendarEvent calendarEvent, int userId)
        {
            if (calendarEvent.ProjectId != null)
            {
                var project = await _context.Projects.FindAsync(calendarEvent.ProjectId);
                await VerifyProject(project, (int)calendarEvent.ProjectId, userId);
            }

            calendarEvent.UserId = userId;

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            return calendarEvent;
        }

        private async Task VerifyProject(Project project, int projectId, int userId)
        {
            // verify project exists
            if (project == null) throw new ProjectNotFoundException((int) projectId);

            // verify user owns project
            if (project.UserId != userId) throw new UserUnauthorizedException(userId);
        }

        public async Task<bool> UpdateEvent(CalendarEvent calendarEvent, int userId)
        {
            var existingCalendarEvent = await _context.CalendarEvents.FindAsync(calendarEvent.Id);
            if (existingCalendarEvent == null) return false;

            if (calendarEvent.UserId != userId)
            {
                throw new UserUnauthorizedException(userId);
            }

            // Map through event fields
            existingCalendarEvent.Description = calendarEvent.Description;
            existingCalendarEvent.StartTime = calendarEvent.StartTime;
            existingCalendarEvent.EndTime = calendarEvent.EndTime;

            if (calendarEvent.ProjectId == null && existingCalendarEvent.ProjectId != null)
            {
                // remove project
                existingCalendarEvent.Project = null;
            }

            if (calendarEvent.ProjectId != null && calendarEvent.ProjectId != existingCalendarEvent.ProjectId)
            {
                // Add project
                var project = await _context.Projects.FindAsync(calendarEvent.ProjectId);

                await VerifyProject(project, (int)calendarEvent.ProjectId, userId);

                existingCalendarEvent.Project = project;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteEvent(int calendarEventId, int userId)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(calendarEventId);
            if (calendarEvent == null) return false;

            if (calendarEvent.UserId != userId)
            {
                throw new UserUnauthorizedException(userId);
            }

            _context.Remove(calendarEvent);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
