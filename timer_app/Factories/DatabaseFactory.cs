using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app.Factories
{
    public static class DatabaseFactory
    {
        public static Project ToDb(this CreateProjectRequest request, int userId)
        {
            return new Project
            {
                Description = request.Description,
                DisplayColour = request.DisplayColour,
                UserId = userId,
            };
        }

        public static CalendarEvent ToDb(this CreateEventRequest request, int userId)
        {
            return new CalendarEvent
            {
                ProjectId = request.ProjectId,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                UserId = userId
            };
        }
    }
}
