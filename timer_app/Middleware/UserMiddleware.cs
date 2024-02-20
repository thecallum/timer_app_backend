using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection.PortableExecutable;
using timer_app.Middleware.Interfaces;
using System.Security.Claims;

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

        public async Task Invoke(HttpContext httpContext, IUserService userService, ITokenValidator tokenValidator)
        {
            _logger.LogInformation("Invoking UserMiddlware");

            var idToken = httpContext.Request.Headers[HeaderConfig.IdToken].ToString();

            if (string.IsNullOrEmpty(idToken))
            {
                httpContext.Response.StatusCode = 403;
                await httpContext.Response.WriteAsync($"Invalid {HeaderConfig.IdToken}");
                return;
            }

            try
            {
                var principal = await tokenValidator.ValidateIdToken(idToken);

                await userService.InitialiseUser(principal);
                httpContext.User = userService.GetUser();

                await _next(httpContext);
            }
            catch (Exception)
            {
                httpContext.Response.StatusCode = 403;
                await httpContext.Response.WriteAsync($"Invalid {HeaderConfig.IdToken}");
                return;
            }
        }
    }
}
