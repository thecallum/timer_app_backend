using System.Security.Claims;
using timer_app.Domain;
using timer_app.Gateways.Interfaces;
using timer_app.Middleware.Interfaces;

namespace timer_app.Middleware
{
    public class UserService : IUserService
    {
        private readonly IUserGateway _userGateway;

        public UserService(IUserGateway userGateway)
        {
            _userGateway = userGateway;
        }

        private ClaimsPrincipal? User;

        public async Task InitialiseUser(string accessToken)
        {
            var userData = await _userGateway.GetUserData(accessToken);
            if (userData == null) return;

            var identity = InitialiseIdentity(userData);

            var user = new ClaimsPrincipal(identity);

            User = user;
        }

        private static ClaimsIdentity InitialiseIdentity(Auth0User userData)
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(ClaimTypes.Name, userData.Name));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, userData.Nickname));
            identity.AddClaim(new Claim(ClaimTypes.Email, userData.Email));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userData.Sub));

            return identity;
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
