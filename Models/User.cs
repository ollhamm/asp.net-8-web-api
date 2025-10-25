using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace aspnetcoreapi.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string? Name { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }


        [JsonIgnore]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [JsonIgnore]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
