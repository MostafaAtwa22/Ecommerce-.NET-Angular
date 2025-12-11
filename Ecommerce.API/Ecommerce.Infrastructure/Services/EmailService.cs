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

        public async Task<bool> SendResetPasswordEmailAsync(string? toEmail, string resetLink)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning($"Invalid email address: {toEmail}");
                return false;
            }

            var email = CreateResetPasswordEmail(toEmail, resetLink);

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls
            );

            await smtpClient.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);

            _logger.LogInformation($"Reset password email sent to {toEmail}");
            return true;
        }

        private MimeMessage CreateResetPasswordEmail(string toEmail, string resetLink)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Reset Your Password";

            var body = GetResetPasswordEmailBody(resetLink);
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            return email;
        }

        private string GetResetPasswordEmailBody(string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Reset Your Password</title>
                    <style>
                        body {{
                            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
                            line-height: 1.6;
                            color: #0c111d;
                            margin: 0;
                            padding: 0;
                            background-color: #f7f9fa;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: white;
                            border-radius: 8px;
                            border: 1px solid #d1d7dc;
                            overflow: hidden;
                            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                        }}
                        .email-header {{
                            background-color: #5624d0;
                            color: white;
                            padding: 2rem;
                            text-align: center;
                        }}
                        .email-header h1 {{
                            margin: 0;
                            font-size: 1.75rem;
                            font-weight: 600;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            gap: 0.5rem;
                        }}
                        .email-header i {{
                            font-size: 1.5rem;
                        }}
                        .email-body {{
                            padding: 2rem;
                        }}
                        .email-body h2 {{
                            margin: 0 0 1rem 0;
                            font-size: 1.25rem;
                            font-weight: 600;
                            color: #0c111d;
                        }}
                        .email-body p {{
                            margin: 0 0 1.5rem 0;
                            font-size: 0.875rem;
                            color: #6a6f73;
                        }}
                        .reset-button {{
                            display: inline-block;
                            background-color: #5624d0;
                            text-decoration: none;
                            padding: 0.75rem 1.5rem;
                            border-radius: 4px;
                            font-weight: 600;
                            font-size: 0.875rem;
                            text-align: center;
                            transition: all 0.2s ease;
                            border: none;
                            cursor: pointer;
                            margin: 1rem 0;
                            color: white !important;
                        }}
                        .reset-button:hover {{
                            background-color: #401b9c;
                        }}
                        .security-notice {{
                            background-color: rgba(86, 36, 208, 0.05);
                            border: 1px solid rgba(86, 36, 208, 0.1);
                            border-radius: 4px;
                            padding: 1rem;
                            margin: 1.5rem 0;
                        }}
                        .security-notice h3 {{
                            margin: 0 0 0.5rem 0;
                            font-size: 0.875rem;
                            font-weight: 600;
                            color: #5624d0;
                            display: flex;
                            align-items: center;
                            gap: 0.5rem;
                        }}
                        .security-notice ul {{
                            margin: 0;
                            padding-left: 1.25rem;
                        }}
                        .security-notice li {{
                            font-size: 0.75rem;
                            color: #6a6f73;
                            margin-bottom: 0.25rem;
                        }}
                        .link-backup {{
                            background-color: #f7f9fa;
                            border: 1px solid #d1d7dc;
                            border-radius: 4px;
                            padding: 1rem;
                            margin: 1.5rem 0;
                            font-size: 0.75rem;
                            color: #6a6f73;
                            word-break: break-all;
                        }}
                        .email-footer {{
                            border-top: 1px solid #d1d7dc;
                            padding: 1.5rem 2rem;
                            text-align: center;
                            font-size: 0.75rem;
                            color: #6a6f73;
                        }}
                        .email-footer a {{
                            color: #5624d0;
                            text-decoration: none;
                        }}
                        @media (max-width: 600px) {{
                            .email-container {{
                                border-radius: 0;
                                border: none;
                            }}
                            .email-header, .email-body, .email-footer {{
                                padding: 1.5rem;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>
                                🔐 Password Reset
                            </h1>
                        </div>
                        
                        <div class='email-body'>
                            <h2>Hello,</h2>
                            
                            <p>We received a request to reset the password for your account. To proceed with resetting your password, click the button below:</p>
                            
                            <div style='text-align: center;'>
                                <a href='{resetLink}' class='reset-button'>
                                    Reset My Password
                                </a>
                            </div>
                            
                            <div class='security-notice'>
                                <h3>
                                    <span>🛡️</span> Security Notice
                                </h3>
                                <ul>
                                    <li>This link will expire in <strong>24 hours</strong></li>
                                    <li>If you didn't request this password reset, please ignore this email</li>
                                    <li>Never share your password or this link with anyone</li>
                                </ul>
                            </div>
                            
                            <p>If the button above doesn't work, you can copy and paste the following link into your browser:</p>
                            
                            <div class='link-backup'>
                                {resetLink}
                            </div>
                            
                            <p>If you're having trouble or didn't request a password reset, please contact our support team immediately.</p>
                            
                            <p>Best regards,<br>
                            <strong>The Team</strong></p>
                        </div>
                        
                        <div class='email-footer'>
                            <p>
                                © {DateTime.Now.Year} Ecommerce. All rights reserved.<br>
                                This email was sent to you because you requested a password reset.<br>
                                <a href='#'>Privacy Policy</a> • <a href='#'>Contact Support</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}