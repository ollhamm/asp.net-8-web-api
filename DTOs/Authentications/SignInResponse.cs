using System;

namespace aspnetcoreapi.DTOs.Auth
{
    public class SignInResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = default!;
    }
}