using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductUpdateDto
    {
        [MinLength(3), MaxLength(50)]
        public string? Name { get; set; } = string.Empty;

        [MinLength(10), MaxLength(500)]
        public string? Description { get; set; } = string.Empty;

        public string? PictureUrl { get; set; } = string.Empty;

        [Range(1, 10_000)]
        public decimal? Price { get; set; }

        public int? ProductTypeId { get; set; }

        public int? ProductBrandId { get; set; }
    }
}