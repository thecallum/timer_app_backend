using timer_app.Boundary.Response;
using timer_app.Infrastructure;

namespace timer_app.Factories
{
    public static class ResponseFactory
    {
        public static ProjectColorResponse ToResponse(this ProjectColor db)
        {
            return new ProjectColorResponse
            {
                Lightest = db.Lightest,
                Light = db.Light,
                Dark = db.Dark,
                Darkest = db.Darkest,
            };
        }

        public static IEnumerable<ProjectResponse> ToResponse(this IEnumerable<ProjectWithCount> db)
        {
            return db.Select(x => x.ToResponse());
        }

        public static ProjectResponse ToResponse(this ProjectWithCount db)
        {
            return new ProjectResponse
            {
                Id = db.Project.Id,
                Description = db.Project.Description,
                ProjectColor = db.Project.ProjectColor.ToResponse(),
                IsActive = db.Project.IsActive,
                TotalEventDurationInMinutes = db.TotalEventDurationInMinutes
            };
        }

        public static ProjectResponse ToResponse(this Project db)
        {
            return new ProjectResponse
            {
                Id = db.Id,
                Description = db.Description,
                ProjectColor = db.ProjectColor.ToResponse(),
                IsActive = db.IsActive,
            };
        }

        public static IEnumerable<CalendarEventResponse> ToResponse(this IEnumerable<CalendarEvent> db)
        {
            return db.Select(x => x.ToResponse());
        }

        public static CalendarEventResponse ToResponse(this CalendarEvent db)
        {
            return new CalendarEventResponse
            {
                Id = db.Id,
                ProjectId = db.ProjectId,
                StartTime = db.StartTime,
                EndTime = db.EndTime,
                Description = db.Description,
            };
        }
    }
}
