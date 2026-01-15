using Ecommerce.Core.Entities.Emails;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Ecommerce.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<MailSettings> settings,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(EmailMessage emailMessage)
        {
            if (string.IsNullOrWhiteSpace(emailMessage.To))
            {
                _logger.LogWarning("Invalid email address");
                return false;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(emailMessage.To));
            email.Subject = emailMessage.Subject;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailMessage.HtmlBody
            };

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls
            );

            await smtpClient.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}", emailMessage.To);
            return true;
        }
    }
}