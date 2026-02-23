using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductReviewFromDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 5)]
        public decimal Rating { get; set; }

        [Required, MaxLength(100)]
        public string Headline { get; set; } = string.Empty;

        [Required, MaxLength(3000)]
        public string Comment { get; set; } = string.Empty;
    }
}