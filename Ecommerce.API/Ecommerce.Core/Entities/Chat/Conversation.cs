using System.ComponentModel.DataAnnotations;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Enums;
using Ecommerce.Core.Interfaces;

namespace Ecommerce.Core.Entities.Chat
{
    public class Conversation : BaseEntity, ISoftDelete
    {
        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        public ConversationStatus Status { get; set; } = ConversationStatus.Open;

        [Required]
        public string InitiatedByUserId { get; set; } = string.Empty;

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ClosedAt { get; set; }

        public ApplicationUser InitiatedByUser { get; set; } = default!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public bool IsDeleted { get; set; }
        public DateTime? DateOFDelete { get; set; }
    }
}