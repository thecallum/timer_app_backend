using System.Reflection.PortableExecutable;
using timer_app.Middleware.Interfaces;

namespace timer_app.Middleware
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserMiddleware> _logger;

        public UserMiddleware(RequestDelegate next, ILogger<UserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext, IUserService userService)
        {
            _logger.LogInformation("Invoking UserMiddlware");

            var accessToken = httpContext.Request.Headers["Authorization"].ToString();

            await userService.InitialiseUser(accessToken);
            httpContext.User = userService.GetUser();

            await _next(httpContext);
        }
    }
}
