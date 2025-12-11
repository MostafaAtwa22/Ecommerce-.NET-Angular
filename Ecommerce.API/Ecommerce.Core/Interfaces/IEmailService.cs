namespace Ecommerce.Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendResetPasswordEmailAsync(string? toEmail, string resetLink);
    }
}