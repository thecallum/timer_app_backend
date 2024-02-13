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

        //[HttpGet]
        //[Authorize]
        //[Route("user")]
        //public async Task<IActionResult> GetUser()
        //{
        //    var accessToken = Request.Headers["Authorization"].ToString();
        //    var userData = await _userGateway.GetUserData(accessToken);

        //    if (userData == null)
        //    {
        //        return Unauthorized();
        //    }

        //    return Ok(userData);
        //}

        [HttpGet]
        public async Task<IActionResult> Authorize([FromQuery] string code)
        {
            var accessToken = await _userGateway.AuthorizeUser(code);
            if (accessToken == null) return Unauthorized();

            var cookieOptions = new CookieOptions();
            Response.Cookies.Append("AccessToken", accessToken, cookieOptions);

            return Redirect("http://localhost:3000");
        }
    }
}
