using System.Security.Claims;

namespace timer_app.Middleware.Interfaces
{
    public interface IUserService
    {
        ClaimsPrincipal GetUser();
        Task InitialiseUser(ClaimsPrincipal principal);
        public string GetId();
    }
}
