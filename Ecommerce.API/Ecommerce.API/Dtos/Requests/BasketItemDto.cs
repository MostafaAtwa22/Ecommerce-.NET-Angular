using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class BasketItemDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$",
            ErrorMessage = "Only letters, numbers, spaces, hyphens and underscores are allowed")]
        public string ProductName { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$",
            ErrorMessage = "Only letters, numbers, spaces, hyphens and underscores are allowed")]
        public string Brand { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$",
            ErrorMessage = "Only letters, numbers, spaces, hyphens and underscores are allowed")]
        public string Type { get; set; } = string.Empty;

        public string PictureUrl { get; set; } = string.Empty;

        [Range(minimum: 3.0, maximum: 10_000.0)]
        [Required]
        public decimal Price { get; set; }

        [Range(minimum: 1, maximum: 100_000)]
        [Required]
        public int Quantity { get; set; }
    }
}