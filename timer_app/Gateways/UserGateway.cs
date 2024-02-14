using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using timer_app.Domain;
using timer_app.Gateways.Interfaces;

namespace timer_app.Gateways
{
    public class UserGateway : IUserGateway
    {
        private readonly IConfiguration _configuration;

        public UserGateway(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Auth0User> GetUserData(string accessToken)
        {
            var domain = _configuration.GetValue<string>("Auth0:Domain");

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
            var domain = Environment.GetEnvironmentVariable("Auth0_Domain");
            var clientId = Environment.GetEnvironmentVariable("Auth0_ClientId");
            var clientSecret = Environment.GetEnvironmentVariable("Auth0_ClientSecret");
            var redirect_uri = Environment.GetEnvironmentVariable("Auth0_RedirectUri");

            var client = new RestClient(domain);
            var request = new RestRequest("/oauth/token", Method.Post);

            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&client_id={clientId}&client_secret={clientSecret}&code={code}&redirect_uri={redirect_uri}", ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            // set cookie
            var responseData = JObject.Parse(response.Content);

            var accessToken = responseData["access_token"].ToString();

            return accessToken;
        }
    }
}
