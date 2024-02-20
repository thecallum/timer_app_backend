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

        public async Task Invoke(HttpContext httpContext, IUserService userService)
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
                var principal = await ValidateIdToken(idToken);

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

        private static async Task<ClaimsPrincipal> ValidateIdToken(string idToken)
        {
            var issuer = Environment.GetEnvironmentVariable("Auth0_Domain");
            var audience = Environment.GetEnvironmentVariable("Auth0_AppAudience");

            // Ensure issuer ends with a slash
            if (!issuer.EndsWith("/")) issuer += "/";

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
           $"{issuer}.well-known/openid-configuration",
           new OpenIdConnectConfigurationRetriever(),
           new HttpDocumentRetriever());

            var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

            // Validate the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfig.SigningKeys,

                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(idToken, validationParameters, out var validatedToken);
            return principal;
        }
    }
}
