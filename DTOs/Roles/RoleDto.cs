using System;
namespace aspnetcoreapi.DTOs
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}