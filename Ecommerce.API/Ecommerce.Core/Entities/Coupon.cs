using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Entities
{
    public class Coupon : BaseEntity
    {
        [Required]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public decimal DiscountAmount { get; set; }
        
        [Required]
        public DateTimeOffset ExpiryDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
