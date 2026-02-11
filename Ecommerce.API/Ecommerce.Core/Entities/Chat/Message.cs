using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;

namespace Ecommerce.Core.Entities.Chat
{
    public class Message : BaseEntity, ISoftDelete
    {
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = default!;

        public string ReciverId { get; set; } = string.Empty;
        public ApplicationUser Reciver { get; set; } = default!;
        public bool IsDeleted { get; set; }
        public DateTime? DateOFDelete { get; set; }
        public bool IsEdited { get; set; }
    }
}