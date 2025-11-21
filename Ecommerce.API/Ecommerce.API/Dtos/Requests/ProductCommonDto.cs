using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductCommonDto
    {
        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Only letters, numbers, spaces, hyphens and underscores are allowed")]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(10), MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, Range(1, 10_000)]
        public decimal Price { get; set; }

        [Required]
        [Range(minimum: 5, maximum: 10_000)]
        public int Quantity { get; set; }

        [Required]
        public int ProductTypeId { get; set; }

        [Required]
        public int ProductBrandId { get; set; }
    }
}