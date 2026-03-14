using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Dtos
{
    public class ProductDiscountDto
    {
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        public string? Name { get; set; }

        public DateTimeOffset? ExpirationDate { get; set; }
        
        public bool IsActive { get; set; }
    }
}
