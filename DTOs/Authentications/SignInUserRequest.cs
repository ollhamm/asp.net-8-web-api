using System.ComponentModel.DataAnnotations;

namespace aspnetcoreapi.DTOs.Auth
{
    public class SignInUserRequest
    {
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = default!;

        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = default!;
    }
}