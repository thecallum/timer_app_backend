using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app.Factories
{
    public static class DatabaseFactory
    {
        public static ProjectColor ToDb(this ProjectColorRequest request)
        {
            return new ProjectColor
            {
                Dark = request.Dark,
                Darkest = request.Darkest,
                Light = request.Light,
                Lightest = request.Lightest,
            };
        }

        public static Project ToDb(this CreateProjectRequest request, int userId)
        {
            return new Project
            {
                Description = request.Description,
                ProjectColor = request.ProjectColor?.ToDb(),
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
