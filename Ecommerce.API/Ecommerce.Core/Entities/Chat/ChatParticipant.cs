using System.ComponentModel.DataAnnotations;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Enums;
using Ecommerce.Core.Interfaces;

namespace Ecommerce.Core.Entities.Chat
{
    public class ChatParticipant : BaseEntity, ISoftDelete
    {
        public int ConversationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ParticipantRole Role { get; set; }

        public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;

        public Conversation Conversation { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public bool IsDeleted { get; set; }
        public DateTime? DateOFDelete { get; set; }
    }
}