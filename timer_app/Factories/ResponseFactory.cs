﻿using timer_app.Boundary.Response;
using timer_app.Infrastructure;

namespace timer_app.Factories
{
    public static class ResponseFactory
    {
        public static IEnumerable<ProjectResponse> ToResponse(this IEnumerable<Project> db)
        {
            return db.Select(x => x.ToResponse());
        }

        public static ProjectResponse ToResponse(this Project db)
        {
            return new ProjectResponse
            {
                Id = db.Id,
                Description = db.Description,
                DisplayColour = db.DisplayColour,
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
                Project = db.Project?.ToResponse(),
                StartTime = db.StartTime,
                EndTime = db.EndTime,
                Description = db.Description,
            };
        }
    }
}
