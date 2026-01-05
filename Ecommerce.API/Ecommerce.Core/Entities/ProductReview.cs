using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.Core.Entities.Identity;

namespace Ecommerce.Core.Entities
{
    public class ProductReview : BaseEntity
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public int ProductId { get; set; } 

        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;

        [Range(1, 5)]
        public decimal Rating { get; set; }

        [Required, MaxLength(100)]
        public string Headline { get; set; } = string.Empty;

        [Required, MaxLength(3000)]
        public string Comment { get; set; } = string.Empty;
        
        public Product Product { get; set; } = default!;
        public ApplicationUser ApplicationUser { get; set; } = default!;
    }
}