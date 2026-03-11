using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.Dtos
{
    public class ProductCommonDto
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(10), MaxLength(1500)]
        public string Description { get; set; } = string.Empty;

        [Required, Range(1, 10_000)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        [Required]
        [Range(minimum: 0, maximum: 10_000)]
        public int Quantity { get; set; }

        [Required]
        public int ProductTypeId { get; set; }

        [Required]
        public int ProductBrandId { get; set; }
    }
}
