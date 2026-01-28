using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.Infrastructure.Services
{
    public static class EmailTemplates
    {
        public static string ResetPassword(string resetLink)
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

        public static string ConfirmEmail(string confirmLink, string code)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Confirm Your Email Address</title>
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
                            background: linear-gradient(135deg, #5624d0 0%, #401b9c 100%);
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
                        .confirm-button {{
                            display: inline-block;
                            background: linear-gradient(135deg, #5624d0 0%, #401b9c 100%);
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
                        .confirm-button:hover {{
                            background: linear-gradient(135deg, #401b9c 0%, #2d136d 100%);
                            transform: translateY(-1px);
                            box-shadow: 0 4px 12px rgba(86, 36, 208, 0.3);
                        }}
                        .code-container {{
                            background-color: #f7f9fa;
                            border: 2px dashed #d1d7dc;
                            border-radius: 8px;
                            padding: 1.5rem;
                            margin: 1.5rem 0;
                            text-align: center;
                            font-family: 'Courier New', monospace;
                        }}
                        .verification-code {{
                            font-size: 2rem;
                            font-weight: 700;
                            letter-spacing: 0.5rem;
                            color: #5624d0;
                            margin: 0.5rem 0;
                        }}
                        .code-label {{
                            font-size: 0.75rem;
                            color: #6a6f73;
                            text-transform: uppercase;
                            letter-spacing: 0.1rem;
                            margin-bottom: 0.5rem;
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
                        .steps-container {{
                            background-color: #f7f9fa;
                            border-radius: 8px;
                            padding: 1.5rem;
                            margin: 1.5rem 0;
                        }}
                        .step {{
                            display: flex;
                            align-items: flex-start;
                            margin-bottom: 1rem;
                            padding-bottom: 1rem;
                            border-bottom: 1px solid #e8e8e8;
                        }}
                        .step:last-child {{
                            margin-bottom: 0;
                            padding-bottom: 0;
                            border-bottom: none;
                        }}
                        .step-number {{
                            background-color: #5624d0;
                            color: white;
                            width: 24px;
                            height: 24px;
                            border-radius: 50%;
                            display: flex;
                            align-items: center;
                            justify-content: center;
                            font-size: 0.75rem;
                            font-weight: 600;
                            margin-right: 1rem;
                            flex-shrink: 0;
                        }}
                        .step-content h4 {{
                            margin: 0 0 0.25rem 0;
                            font-size: 0.875rem;
                            font-weight: 600;
                            color: #0c111d;
                        }}
                        .step-content p {{
                            margin: 0;
                            font-size: 0.75rem;
                            color: #6a6f73;
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
                            .verification-code {{
                                font-size: 1.5rem;
                                letter-spacing: 0.25rem;
                            }}
                            .step {{
                                flex-direction: column;
                            }}
                            .step-number {{
                                margin-right: 0;
                                margin-bottom: 0.5rem;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>
                                📧 Verify Your Email
                            </h1>
                        </div>
                        
                        <div class='email-body'>
                            <h2>Welcome to Our Platform!</h2>
                            
                            <p>Thank you for signing up! To complete your registration and activate your account, please verify your email address by clicking the button below:</p>
                            
                            <div style='text-align: center;'>
                                <a href='{confirmLink}' class='confirm-button'>
                                    Verify Email Address
                                </a>
                            </div>
                            
                            <div class='code-container'>
                                <div class='code-label'>Verification Code</div>
                                <div class='verification-code'>{code}</div>
                                <p style='font-size: 0.75rem; color: #6a6f73; margin: 0.5rem 0 0 0;'>
                                    Enter this code on the verification page if the button doesn't work
                                </p>
                            </div>
                            
                            <div class='steps-container'>
                                <div class='step'>
                                    <div class='step-number'>1</div>
                                    <div class='step-content'>
                                        <h4>Verify Your Email</h4>
                                        <p>Click the button above or use the verification code to confirm your email address</p>
                                    </div>
                                </div>
                                <div class='step'>
                                    <div class='step-number'>2</div>
                                    <div class='step-content'>
                                        <h4>Complete Your Profile</h4>
                                        <p>Add your details and preferences to personalize your experience</p>
                                    </div>
                                </div>
                                <div class='step'>
                                    <div class='step-number'>3</div>
                                    <div class='step-content'>
                                        <h4>Start Exploring</h4>
                                        <p>Access all features and services available to verified members</p>
                                    </div>
                                </div>
                            </div>
                            
                            <div class='security-notice'>
                                <h3>
                                    <span>🔒</span> Account Security
                                </h3>
                                <ul>
                                    <li>This verification link will expire in <strong>1 hour</strong></li>
                                    <li>Never share your verification code with anyone</li>
                                    <li>Our team will never ask for your password or verification code</li>
                                    <li>If you didn't create this account, please ignore this email</li>
                                </ul>
                            </div>
                            
                            <p>If the button above doesn't work, you can copy and paste the following link into your browser:</p>
                            
                            <div class='link-backup'>
                                {confirmLink}
                            </div>
                            
                            <p>Having trouble verifying your email? Contact our support team for assistance.</p>
                            
                            <p>Welcome aboard,<br>
                            <strong>The Team</strong></p>
                        </div>
                        
                        <div class='email-footer'>
                            <p>
                                © {DateTime.Now.Year} Ecommerce. All rights reserved.<br>
                                This email was sent to verify your email address for account creation.<br>
                                <a href='#'>Privacy Policy</a> • <a href='#'>Contact Support</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public static string OrderConfirmation(Order order)
        {
            var orderDate = order.OrderDate.ToString("MMMM dd, yyyy");
            var orderTime = order.OrderDate.ToString("hh:mm tt");
            var totalAmount = order.GetTotal();

            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Order Confirmation #{order.Id}</title>
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
                            background: linear-gradient(135deg, #5624d0 0%, #401b9c 100%);
                            color: white;
                            padding: 2rem;
                            text-align: center;
                        }}
                        .email-header h1 {{
                            margin: 0;
                            font-size: 1.75rem;
                            font-weight: 600;
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
                        .status-badge {{
                            display: inline-block;
                            background-color: #e7f7e9;
                            color: #0a7c0a;
                            padding: 0.5rem 1rem;
                            border-radius: 20px;
                            font-weight: 600;
                            font-size: 0.875rem;
                            margin-bottom: 1.5rem;
                        }}
                        .order-info {{
                            background-color: #f7f9fa;
                            border-radius: 8px;
                            padding: 1.5rem;
                            margin: 1.5rem 0;
                        }}
                        .info-row {{
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 0.75rem;
                            font-size: 0.875rem;
                        }}
                        .info-row:last-child {{
                            margin-bottom: 0;
                        }}
                        .info-label {{
                            color: #6a6f73;
                        }}
                        .info-value {{
                            font-weight: 600;
                            color: #0c111d;
                        }}
                        .info-row.total {{
                            border-top: 2px solid #d1d7dc;
                            padding-top: 1rem;
                            margin-top: 1rem;
                            font-size: 1rem;
                        }}
                        .info-row.total .info-value {{
                            color: #5624d0;
                            font-size: 1.25rem;
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
                            .info-row {{
                                flex-direction: column;
                                gap: 0.25rem;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <h1>
                                🛍️ Order Confirmed!
                            </h1>
                        </div>
                        
                        <div class='email-body'>
                            <div class='status-badge'>
                                ✅ Order #{order.Id} Confirmed
                            </div>
                            
                            <h2>Thank you for your order!</h2>
                            
                            <p>Your order has been successfully received and is being processed. We'll notify you once it ships.</p>
                            
                            <div class='order-info'>
                                <div class='info-row'>
                                    <span class='info-label'>Order Number</span>
                                    <span class='info-value'>#{order.Id}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='info-label'>Order Date</span>
                                    <span class='info-value'>{orderDate} at {orderTime}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='info-label'>Order Status</span>
                                    <span class='info-value'>{order.Status}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='info-label'>Subtotal</span>
                                    <span class='info-value'>{order.SubTotal:C}</span>
                                </div>
                                <div class='info-row'>
                                    <span class='info-label'>Delivery</span>
                                    <span class='info-value'>{order.DeliveryMethod?.Price:C}</span>
                                </div>
                                <div class='info-row total'>
                                    <span class='info-label'>Total Amount</span>
                                    <span class='info-value'>{totalAmount:C}</span>
                                </div>
                            </div>
                            
                            <p>You will receive another email with tracking information once your order ships.</p>
                            
                            <p>If you have any questions about your order, please contact our support team.</p>
                            
                            <p>Best regards,<br>
                            <strong>The Team</strong></p>
                        </div>
                        
                        <div class='email-footer'>
                            <p>
                                © {DateTime.Now.Year} Ecommerce. All rights reserved.<br>
                                This email confirms your order #{order.Id}.<br>
                                <a href='#'>Contact Support</a> • <a href='#'>Privacy Policy</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public static string TwoFactorCode(string code)
        {
            return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                                line-height: 1.6;
                                color: #0c111d;
                                background-color: #f7f9fa;
                                margin: 0;
                                padding: 0;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 0 auto;
                                padding: 40px 20px;
                            }}
                            .email-wrapper {{
                                background: white;
                                border-radius: 12px;
                                box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
                                overflow: hidden;
                                border: 1px solid #d1d7dc;
                            }}
                            .header {{
                                background: linear-gradient(135deg, #5624d0 0%, #401b9c 100%);
                                color: white;
                                padding: 30px;
                                text-align: center;
                            }}
                            .logo {{
                                font-size: 28px;
                                font-weight: 700;
                                margin-bottom: 10px;
                                letter-spacing: -0.5px;
                            }}
                            .logo-accent {{
                                color: #a78bfa;
                            }}
                            .title {{
                                font-size: 22px;
                                font-weight: 600;
                                margin: 0;
                            }}
                            .content {{
                                padding: 40px 30px;
                            }}
                            .greeting {{
                                font-size: 16px;
                                color: #6a6f73;
                                margin-bottom: 30px;
                                line-height: 1.7;
                            }}
                            .code-container {{
                                text-align: center;
                                margin: 40px 0;
                            }}
                            .code-label {{
                                font-size: 14px;
                                color: #6a6f73;
                                margin-bottom: 10px;
                                font-weight: 600;
                                text-transform: uppercase;
                                letter-spacing: 0.5px;
                            }}
                            .code-box {{
                                background: linear-gradient(135deg, rgba(86, 36, 208, 0.08) 0%, rgba(159, 122, 234, 0.08) 100%);
                                border: 2px solid rgba(86, 36, 208, 0.2);
                                border-radius: 10px;
                                padding: 25px;
                                display: inline-block;
                                margin: 15px 0;
                                min-width: 300px;
                            }}
                            .code {{
                                font-size: 42px;
                                font-weight: 700;
                                letter-spacing: 8px;
                                color: #5624d0;
                                font-family: 'Monaco', 'Courier New', monospace;
                                text-shadow: 0 2px 4px rgba(86, 36, 208, 0.1);
                            }}
                            .expiry {{
                                font-size: 14px;
                                color: #6a6f73;
                                margin-top: 15px;
                                font-weight: 500;
                            }}
                            .expiry-highlight {{
                                color: #5624d0;
                                font-weight: 600;
                            }}
                            .warning-box {{
                                background: rgba(237, 137, 54, 0.08);
                                border-left: 4px solid #ed8936;
                                border-radius: 4px;
                                padding: 20px;
                                margin-top: 40px;
                            }}
                            .warning-title {{
                                color: #ed8936;
                                font-size: 16px;
                                font-weight: 600;
                                margin-bottom: 10px;
                                display: flex;
                                align-items: center;
                                gap: 8px;
                            }}
                            .warning-icon {{
                                font-size: 18px;
                            }}
                            .warning-content {{
                                color: #742a2a;
                                font-size: 14px;
                                line-height: 1.6;
                                margin: 0;
                            }}
                            .footer {{
                                background: #f7f9fa;
                                border-top: 1px solid #d1d7dc;
                                padding: 25px 30px;
                                text-align: center;
                                color: #6a6f73;
                            }}
                            .footer-text {{
                                font-size: 12px;
                                margin-bottom: 10px;
                                line-height: 1.5;
                            }}
                            .support-link {{
                                color: #5624d0;
                                text-decoration: none;
                                font-weight: 600;
                            }}
                            .button {{
                                display: inline-block;
                                background: #5624d0;
                                color: white;
                                padding: 14px 32px;
                                border-radius: 6px;
                                text-decoration: none;
                                font-weight: 600;
                                font-size: 15px;
                                margin-top: 20px;
                                transition: all 0.2s ease;
                            }}
                            .button:hover {{
                                background: #401b9c;
                                transform: translateY(-1px);
                                box-shadow: 0 4px 12px rgba(86, 36, 208, 0.2);
                            }}
                            .security-note {{
                                font-size: 13px;
                                color: #6a6f73;
                                margin-top: 30px;
                                padding-top: 20px;
                                border-top: 1px dashed #d1d7dc;
                                line-height: 1.6;
                            }}
                            .divider {{
                                height: 1px;
                                background: linear-gradient(90deg, transparent, #d1d7dc, transparent);
                                margin: 30px 0;
                            }}
                            @media (max-width: 480px) {{
                                .container {{
                                    padding: 20px 15px;
                                }}
                                .header {{
                                    padding: 25px 20px;
                                }}
                                .content {{
                                    padding: 30px 20px;
                                }}
                                .code-box {{
                                    min-width: 250px;
                                    padding: 20px;
                                }}
                                .code {{
                                    font-size: 36px;
                                    letter-spacing: 6px;
                                }}
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='email-wrapper'>
                                <div class='header'>
                                    <div class='logo'>E<span class='logo-accent'>Shop</span></div>
                                    <h1 class='title'>Two-Factor Authentication</h1>
                                </div>
                                
                                <div class='content'>
                                    <p class='greeting'>
                                        Hello,<br><br>
                                        You are attempting to log in to your EShop account. 
                                        To complete the sign-in process, please use the verification code below:
                                    </p>
                                    
                                    <div class='code-container'>
                                        <div class='code-label'>Your Verification Code</div>
                                        <div class='code-box'>
                                            <div class='code'>{code}</div>
                                        </div>
                                        <p class='expiry'>
                                            This code will expire in <span class='expiry-highlight'>10 minutes</span>.
                                        </p>
                                    </div>
                                    
                                    <div class='divider'></div>
                                    
                                    <p style='text-align: center;'>
                                        <a href='#' class='button'>Log In Now</a>
                                    </p>
                                    
                                    <div class='security-note'>
                                        <strong>Important:</strong> For your security, never share this code with anyone. 
                                        EShop staff will never ask for your verification code.
                                    </div>
                                    
                                    <div class='warning-box'>
                                        <div class='warning-title'>
                                            <span class='warning-icon'>⚠</span>
                                            Security Notice
                                        </div>
                                        <p class='warning-content'>
                                            If you did not attempt to log in, please ignore this email and consider 
                                            <strong>changing your password immediately</strong>. Your account security is important to us.
                                        </p>
                                    </div>
                                </div>
                                
                                <div class='footer'>
                                    <p class='footer-text'>
                                        This is an automated message from EShop. Please do not reply to this email.
                                    </p>
                                    <p class='footer-text'>
                                        Need help? Contact our <a href='mailto:support@eshop.com' class='support-link'>support team</a>
                                    </p>
                                    <p class='footer-text' style='font-size: 11px; opacity: 0.7; margin-top: 15px;'>
                                        © {DateTime.Now.Year} EShop. All rights reserved.
                                    </p>
                                </div>
                            </div>
                        </div>
                    </body>
                    </html>";
        }
    }
}