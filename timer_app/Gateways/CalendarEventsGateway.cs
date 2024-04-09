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

        public async Task<IEnumerable<CalendarEvent>> GetAllEvents(string userId, DateTime? startTime, DateTime? endTime)
        {
            var query = _context.CalendarEvents
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            if (startTime != null)
            {
                // event occurs within window
                query = query.Where(x => x.StartTime < endTime && x.EndTime > startTime);
            }

            return await query.ToListAsync();
        }

        public async Task<CalendarEvent> CreateEvent(CreateEventRequest request, string userId)
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

        private static void VerifyProject(Project project, int projectId, string userId)
        {
            // verify project exists
            if (project == null)
            {
                throw new ProjectNotFoundException((int)projectId);
            }

            // verify user owns project
            if (project.UserId != userId)
            {
                throw new UserUnauthorizedToAccessProjectException(userId);
            }

            // verify project is active 
            // (cannot assign an archived project)
            if (!project.IsActive)
            {
                throw new ProjectIsArchivedException(projectId);
            }
        }

        public async Task<CalendarEvent> UpdateEvent(int calendarEventId, UpdateEventRequest request, string userId)
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
                existingCalendarEvent.ProjectId = null;
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

        public async Task<bool> DeleteEvent(int calendarEventId, string userId)
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
