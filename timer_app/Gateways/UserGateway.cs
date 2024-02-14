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
            var domain = Environment.GetEnvironmentVariable("Auth0_Domain");

            var client = new RestClient(domain);
            var request = new RestRequest("/userinfo", Method.Get);

            request.AddHeader("Authorization", accessToken);

            var response = await client.ExecuteAsync(request);

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

        public async Task<string> AuthorizeUser(string code)
        {
            _logger.LogInformation("Calling UserGateway.AuthorizeUser");

            var domain = Environment.GetEnvironmentVariable("Auth0_Domain");
            var clientId = Environment.GetEnvironmentVariable("Auth0_ClientId");
            var clientSecret = Environment.GetEnvironmentVariable("Auth0_ClientSecret");
            var redirect_uri = Environment.GetEnvironmentVariable("Auth0_RedirectUri");

            _logger.LogInformation("Setting up request in UserGateway.AuthorizeUser");

            var options = new RestClientOptions { 
                BaseUrl = new Uri(domain, UriKind.Relative),
                ThrowOnAnyError = true,
                
            };

            var client = new RestClient(options);
            var request = new RestRequest("/oauth/token", Method.Post);

            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&client_id={clientId}&client_secret={clientSecret}&code={code}&redirect_uri={redirect_uri}", ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            _logger.LogInformation("Response recieved {Response} in UserGateway.AuthorizeUser with {StatusCode}", response.Content, response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(response.ErrorMessage);
            }

            // set cookie
            var responseData = JObject.Parse(response.Content);

            var accessToken = responseData["access_token"].ToString();

            return accessToken;
        }
    }
}
