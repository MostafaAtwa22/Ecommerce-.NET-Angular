using Ecommerce.Core.Entities.Chat;
using Ecommerce.Core.Entities.orderAggregate;
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
        public string? GoogleId { get; set; }

        public Address? Address { get; set; } = new();
        public ICollection<Conversation> InitiatedConversations { get; set; } = [];
        public ICollection<Message> MessagesSent { get; set; } = [];
        public ICollection<ChatParticipant> ChatParticipations { get; set; } = [];        
        public ICollection<ProductReview> ProductReviews { get; set; } = [];
        public ICollection<Order> Orders { get; set; } = []; 
        public ICollection<RefreshToken>? RefreshTokens { get; set; } = []; 
    }
}
