using timer_app.Domain;
using timer_app.Gateways.Interfaces;

namespace timer_app_tests
{
    public class MockUserGateway : IUserGateway
    {
        private readonly Auth0User UserData;

        public MockUserGateway(Auth0User userData)
        {
            UserData = userData;
        }

        public Task<string> AuthorizeUser(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<Auth0User> GetUserData(string accessToken)
        {
            return UserData;
        }
    }
}
