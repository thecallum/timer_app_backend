using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using timer_app.Domain;
using timer_app.Gateways.Interfaces;

namespace timer_app.Gateways
{
    public class UserGateway : IUserGateway
    {
        private readonly ILogger<IUserGateway> _logger;

        public UserGateway(ILogger<IUserGateway> logger)
        {
            _logger = logger;
        }

        public async Task<Auth0User> GetUserData(string accessToken)
        {
            _logger.LogInformation("UserGateway about to fetch user data");

            var domain = Environment.GetEnvironmentVariable("Auth0_Domain");

            var options = new RestClientOptions
            {
                BaseUrl = new Uri(domain),
                ThrowOnAnyError = true
            };

            var client = new RestClient(options);
            var request = new RestRequest("/userinfo", Method.Get);

            request.AddHeader("Authorization", accessToken);

            _logger.LogInformation("UserGateway is now fetching user data");

            var response = await client.ExecuteAsync(request);

            _logger.LogInformation("UserGateway fetched user data with {Response} {Status}", response.Content, response.StatusCode);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var responseData = JObject.Parse(response.Content);

            return new Auth0User
            {
                Id = responseData["sub"].ToString(),
                Name = responseData["name"].ToString(),
                Email = responseData["email"].ToString(),
            };
        }
    }
}
