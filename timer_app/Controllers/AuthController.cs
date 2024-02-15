using Microsoft.AspNetCore.Mvc;
using timer_app.Gateways.Interfaces;

namespace timer_app.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserGateway _userGateway;

        public AuthController(IUserGateway userGateway)
        {
            _userGateway = userGateway;
        }

        [HttpGet]
        public async Task<IActionResult> Authorize([FromQuery] string code)
        {
            try
            {
                var accessToken = await _userGateway.AuthorizeUser(code);
                if (accessToken == null)
                {
                    // unauthorized
                    // return Unauthorized();
                    return Redirect("http://localhost:3000");
                }

                // var cookieOptions = new CookieOptions();
                // Response.Cookies.Append("AccessToken", accessToken, cookieOptions);

                var redirectUrl = $"http://localhost:3000/api/authorize?accessToken={accessToken}";

                return Redirect(redirectUrl);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
