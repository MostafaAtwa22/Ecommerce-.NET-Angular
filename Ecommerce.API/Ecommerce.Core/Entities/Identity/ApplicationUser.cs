using Ecommerce.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Address? Address { get; set; } = new();
        public ICollection<ProductReview> ProductReviews { get; set; } = [];
    }
}
