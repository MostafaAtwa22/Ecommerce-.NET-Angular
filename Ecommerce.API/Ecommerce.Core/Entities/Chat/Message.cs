using System.ComponentModel.DataAnnotations;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;

namespace Ecommerce.Core.Entities.Chat
{
    public class Message : BaseEntity, ISoftDelete
    {
        public int ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required, MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTimeOffset? ReadAt { get; set; }

        public Conversation Conversation { get; set; } = default!;
        public ApplicationUser Sender { get; set; } = default!;

        public bool IsDeleted { get; set; }
        public DateTime? DateOFDelete { get; set; }
    }
}