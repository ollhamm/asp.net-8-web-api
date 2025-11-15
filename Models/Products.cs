using System.ComponentModel.DataAnnotations;
namespace aspnetcoreapi.Models
{
    public class Product
    {
       [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string? Name { get; set; }

        public decimal Price { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
