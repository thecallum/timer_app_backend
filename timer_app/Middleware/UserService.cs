using System.Security.Claims;
using timer_app.Middleware.Interfaces;

namespace timer_app.Middleware
{
    public class UserService : IUserService
    {
        private ClaimsPrincipal? User;

        public async Task InitialiseUser(ClaimsPrincipal principal)
        {
            User = principal;
        }

        public ClaimsPrincipal GetUser()
        {
            return User;
        }

        public string GetId()
        {
            var identity = User.Identities.FirstOrDefault();
            var claims = identity.Claims.ToList();

            var userId = claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            return userId;
        }
    }
}
