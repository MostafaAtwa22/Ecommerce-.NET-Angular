using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Responses;

namespace Ecommerce.API.Hubs
{
    public interface IChatClient
    {
        Task ReceiveNewMessage(MessageResponseDto messageResponseDto);
        Task ReceiveMessageList(Pagination<MessageResponseDto> messages);
        Task Notify(ProfileResponseDto user);
        Task NotifyTypingToUser(string senderUserName);
        Task OnlineUsers(IEnumerable<OnlineUserDto> users);
        Task MessageEdited(MessageResponseDto message);
        Task MessageDeleted(int messageId);
    }
}