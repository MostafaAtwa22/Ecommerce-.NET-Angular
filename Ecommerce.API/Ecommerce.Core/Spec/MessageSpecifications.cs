using Ecommerce.Core.Entities.Chat;
using Ecommerce.Core.Params;

namespace Ecommerce.Core.Spec
{
    public static class MessageSpecifications
    {
        public static ISpecifications<Message> BuildChatHistorySpec(MessageSpecParams specParams)
        {
            return new SpecificationBuilder<Message>()
                .Where(x => (x.ReciverId == specParams.ReceiverId && x.SenderId == specParams.SenderId) ||
                            (x.ReciverId == specParams.SenderId && x.SenderId == specParams.ReceiverId))
                .OrderByDesc(x => x.CreatedAt)
                .Paginate(specParams.PageIndex, specParams.PageSize);
        }

        public static ISpecifications<Message> BuildChatHistoryCountSpec(MessageSpecParams specParams)
        {
            return new SpecificationBuilder<Message>()
                .Where(x => (x.ReciverId == specParams.ReceiverId && x.SenderId == specParams.SenderId) ||
                            (x.ReciverId == specParams.SenderId && x.SenderId == specParams.ReceiverId));
        }

        public static ISpecifications<Message> BuildUnreadMessagesSpec(string currentUserId, string senderId)
        {
            return new SpecificationBuilder<Message>()
                .Where(m => m.ReciverId == currentUserId && m.SenderId == senderId && !m.IsRead);
        }

        public static ISpecifications<Message> BuildUnreceivedMessagesSpec(string receiverId)
        {
            return new SpecificationBuilder<Message>()
                .Where(m => m.ReciverId == receiverId && !m.IsReceived);
        }
    }
}
