using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.Core.Entities.Identity;

namespace Ecommerce.Core.Entities
{
    public class ProductReview : BaseEntity
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public int ProductId { get; set; } 

        [Range(1, 5)]
        public decimal Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Product Product { get; set; } = default!;
        public ApplicationUser ApplicationUser { get; set; } = default!;
    }
}