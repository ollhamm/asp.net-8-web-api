using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.DTOs.Auth;
using aspnetcoreapi.Services;

namespace aspnetcoreapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) => _auth = auth;

        // POST: api/auth/signup
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpUserRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var created = await _auth.SignUpAsync(req, ct);
                return Created($"/api/users/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST: api/auth/signin
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInUserRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {

                var result = await _auth.SignInAsync(req, ct);
                if (result.AccessToken != null)
                {
                    Response.Cookies.Append("access_token", result.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        Path = "/"
                    });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            // Hapus cookie access_token
            Response.Cookies.Delete("access_token");
            return Ok(new { message = "Logout successful" });
        }
    }
}