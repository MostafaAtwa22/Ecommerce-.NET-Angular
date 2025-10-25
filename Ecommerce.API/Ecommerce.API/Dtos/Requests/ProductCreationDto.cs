using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductCreationDto
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(10), MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string PictureUrl { get; set; } = string.Empty;

        [Required, Range(1, 10_000)]
        public decimal Price { get; set; }

        [Required]
        public int ProductTypeId { get; set; }

        [Required]
        public int ProductBrandId { get; set; }
    }
}