using System.Reflection.PortableExecutable;
using timer_app.Middleware.Interfaces;

namespace timer_app.Middleware
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IUserService userService)
        {
            var accessToken = httpContext.Request.Headers["Authorization"].ToString();

            await userService.InitialiseUser(accessToken);
            httpContext.User = userService.GetUser();

            await _next(httpContext);
        }
    }
}
