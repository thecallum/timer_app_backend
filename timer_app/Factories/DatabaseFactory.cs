using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app.Factories
{
    public static class DatabaseFactory
    {
        public static Project ToDb(this CreateProjectRequest request)
        {
            return new Project
            {
                Description = request.Description,
                DisplayColour = request.DisplayColour,
            };
        }
    }
}
