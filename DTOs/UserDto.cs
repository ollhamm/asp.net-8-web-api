using System;
namespace aspnetcoreapi.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string>? Roles { get; set; }
    }
}