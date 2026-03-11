using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class CouponCreateDto
    {
        [Required, MinLength(3), MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        [Required]
        public DateTimeOffset ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
