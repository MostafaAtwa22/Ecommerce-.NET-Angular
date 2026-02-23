namespace Ecommerce.Core.Interfaces
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(string userMessage);
    }
}
