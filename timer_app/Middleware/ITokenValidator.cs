using System.Security.Claims;

namespace timer_app.Middleware
{
    public interface ITokenValidator
    {
        Task<ClaimsPrincipal> ValidateIdToken(string idToken);
    }
}
