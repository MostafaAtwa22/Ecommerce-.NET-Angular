using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductReviewCreationDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 5)]
        public decimal Rating { get; set; }

        [MaxLength(3000)]
        public string? Comment { get; set; }
    }
}