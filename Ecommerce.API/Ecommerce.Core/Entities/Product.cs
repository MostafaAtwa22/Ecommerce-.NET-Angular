using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Entities
{
    public class Product : BaseEntity
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(10), MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string PictureUrl { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public int ProductBrandId { get; set; }
        public ProductBrand ProductBrand { get; set; } = default!;

        public int ProductTypeId { get; set; }
        public ProductType ProductType { get; set; } = default!;
    }
}