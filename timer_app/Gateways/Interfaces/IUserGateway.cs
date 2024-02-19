using timer_app.Domain;

namespace timer_app.Gateways.Interfaces
{
    public interface IUserGateway
    {
        Task<Auth0User> GetUserData(string accessToken);
    }
}
