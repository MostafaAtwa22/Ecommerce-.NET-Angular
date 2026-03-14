using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Entities
{
    public class ProductDiscount
    {
        [Range(0, 100)]
        public decimal Percentage { get; set; } = 0;

        public string? Name { get; set; }

        public DateTimeOffset? ExpirationDate { get; set; }

        public bool IsActive => Percentage > 0 && 
                                (!ExpirationDate.HasValue || ExpirationDate.Value > DateTimeOffset.UtcNow);

        public decimal CalculateDiscountedPrice(decimal originalPrice)
        {
            if (!IsActive) return originalPrice;
            return originalPrice - (originalPrice * Percentage / 100);
        }
    }
}
