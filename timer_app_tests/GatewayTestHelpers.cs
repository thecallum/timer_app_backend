using timer_app.Infrastructure;

namespace timer_app_tests
{
    public static class GatewayTestHelpers
    {
        public static async Task AddProjectsToDb(params Project[] projects)
        {
            MockDbContext.Instance.Projects.AddRange(projects);
            await MockDbContext.Instance.SaveChangesAsync();
        }

        public static async Task AddEventsToDb(params CalendarEvent[] events)
        {
            MockDbContext.Instance.CalendarEvents.AddRange(events);
            await MockDbContext.Instance.SaveChangesAsync();
        }

        public static async Task<CalendarEvent> GetEvent(int calendarEventId)
        {
            return await MockDbContext.Instance.CalendarEvents.FindAsync(calendarEventId);
        }

        public static async Task<Project> GetProject(int projectId)
        {
            return await MockDbContext.Instance.Projects.FindAsync(projectId);

        }
    }
}
