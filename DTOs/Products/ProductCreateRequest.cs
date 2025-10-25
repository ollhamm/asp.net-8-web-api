using System.ComponentModel.DataAnnotations;

namespace aspnetcoreapi.DTOs
{
    public class ProductCreateRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Price must be a valid decimal number with up to two decimal places.")]
        public decimal Price { get; set; }
    }
}