using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace aspnetcoreapi.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}