using System.ComponentModel.DataAnnotations;

namespace aspnetcoreapi.DTOs
{
    public class RoleCreateRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;
    }
}