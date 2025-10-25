using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspnetcoreapi.Models
{
    public class UserRole
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = default!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}