
namespace Ecommerce.Core.Entities.Chat
{
    public class Message : BaseEntity, ISoftDelete
    {
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsReceived { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = default!;

        public string ReciverId { get; set; } = string.Empty;
        public ApplicationUser Reciver { get; set; } = default!;
        public bool IsDeleted { get; set; }
        public DateTime? DateOFDelete { get; set; }
        public bool IsEdited { get; set; }

        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachmentType { get; set; }
    }
}
