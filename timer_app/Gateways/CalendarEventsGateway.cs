using Microsoft.EntityFrameworkCore;
using timer_app.Boundary.Request;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.Infrastructure;
using timer_app.Infrastructure.Exceptions;

namespace timer_app.Gateway
{
    public class CalendarEventsGateway : ICalendarEventsGateway
    {
        private readonly TimerAppDbContext _context;

        public CalendarEventsGateway(TimerAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CalendarEvent>> GetAllEvents(DateTime startTime, DateTime endTime, int userId)
        {
            var query = _context.CalendarEvents
                .Include(x => x.Project)
                .Where(x => x.UserId == userId);

            // event occurs within window
            query = query
                .Where(x =>
                    (x.StartTime < startTime && x.EndTime > endTime) || // event starts before, and ends after
                    (x.StartTime > startTime && x.StartTime < endTime) || // events starts within window
                    (x.EndTime > startTime && x.EndTime < endTime)  // events ends within window
                );

            return await query.ToListAsync();
        }

        public async Task<CalendarEvent> CreateEvent(CreateEventRequest request, int userId)
        {
            var calendarEvent = request.ToDb(userId);

            if (request.ProjectId != null)
            {
                var project = await _context.Projects.FindAsync(request.ProjectId);
                VerifyProject(project, (int)request.ProjectId, userId);
            }

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            return calendarEvent;
        }

        private static void VerifyProject(Project project, int projectId, int userId)
        {
            // verify project exists
            if (project == null) throw new ProjectNotFoundException((int)projectId);

            // verify user owns project
            if (project.UserId != userId) throw new UserUnauthorizedToAccessProjectException(userId);
        }

        public async Task<CalendarEvent> UpdateEvent(int calendarEventId, UpdateEventRequest request, int userId)
        {
            var existingCalendarEvent = await _context.CalendarEvents.FindAsync(calendarEventId);
            if (existingCalendarEvent == null) return null;

            if (existingCalendarEvent.UserId != userId)
            {
                throw new UserUnauthorizedToAccessEventException(userId);
            }

            // Map through event fields
            existingCalendarEvent.Description = request.Description;
            existingCalendarEvent.StartTime = request.StartTime;
            existingCalendarEvent.EndTime = request.EndTime;

            if (request.ProjectId == null && existingCalendarEvent.ProjectId != null)
            {
                // remove project
                existingCalendarEvent.Project = null;
            }

            if (request.ProjectId != null && existingCalendarEvent.ProjectId != request.ProjectId)
            {
                // Add project
                var project = await _context.Projects.FindAsync(request.ProjectId);

                VerifyProject(project, (int)request.ProjectId, userId);

                existingCalendarEvent.Project = project;
            }

            await _context.SaveChangesAsync();

            return existingCalendarEvent;
        }

        public async Task<bool> DeleteEvent(int calendarEventId, int userId)
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(calendarEventId);
            if (calendarEvent == null) return false;

            if (calendarEvent.UserId != userId)
            {
                throw new UserUnauthorizedToAccessEventException(userId);
            }

            _context.Remove(calendarEvent);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
