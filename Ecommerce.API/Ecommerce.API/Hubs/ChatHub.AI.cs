using Microsoft.AspNetCore.SignalR;

namespace Ecommerce.API.Hubs
{
    public partial class ChatHub
    {
        [AllowAnonymous]
        public async Task AskBot(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new HubException("Message cannot be empty");

            var response = await _chatbotService.GetResponseAsync(message);
            await Clients.Caller.ReceiveNewMessage(new MessageResponseDto
            {
                Id = 0, // No DB ID
                SenderId = "BOT",
                Content = response,
                ReciverId = Context.UserIdentifier!,
                CreatedAt = DateTime.UtcNow,
                IsRead = true
            });
        }
    }
}
