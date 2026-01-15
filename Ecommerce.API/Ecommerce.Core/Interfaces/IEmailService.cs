using Ecommerce.Core.Entities.Emails;

namespace Ecommerce.Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(EmailMessage email);
    }
}