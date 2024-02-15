using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using timer_app.Domain;
using timer_app.Gateways.Interfaces;

namespace timer_app.Gateways
{
    public class UserGateway : IUserGateway
    {
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
    }
}
