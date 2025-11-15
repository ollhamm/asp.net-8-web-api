using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.DTOs.Auth;
using aspnetcoreapi.Services;
using aspnetcoreapi.Common;

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
            var cookieExists = await Task.FromResult(Request.Cookies.ContainsKey("access_token"));
            if (cookieExists)
            {
                Response.Cookies.Append("access_token", "", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(-1),
                });
            }
            return Ok(new { message = "Logout successful" });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var token = await _auth.ForgotPasswordAsync(req.Email);
            
            // HANYA UNTUK DEVELOPMENT TESTING
            #if DEBUG
            return Ok(new ApiResponse
            {
                Title = "Success",
                Status = 200,
                Message = "Password reset link has been sent to your email.",
                Data = new { token = token } // Hanya muncul di development
            });
            #else
            return Ok(new ApiResponse
            {
                Title = "Success",
                Status = 200,
                Message = "Password reset link has been sent to your email.",
                Data = null
            });
            #endif
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            await _auth.ResetPasswordAsync(req.Email, req.Token, req.NewPassword);
            return Ok(new ApiResponse
            {
                Title = "Success",
                Status = 200,
                Message = "Password has been reset successfully.",
                Data = null
            });
        }

    }
}