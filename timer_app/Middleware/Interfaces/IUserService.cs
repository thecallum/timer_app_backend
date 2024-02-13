using System.Security.Claims;

namespace timer_app.Middleware.Interfaces
{
    public interface IUserService
    {
        ClaimsPrincipal GetUser();
        Task InitialiseUser(string accessToken);
        public string GetId();
    }
}
