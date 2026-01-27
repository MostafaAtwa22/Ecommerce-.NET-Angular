using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Entities.Emails;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Ecommerce.Infrastructure.Services;

namespace Ecommerce.API.BackgroundJobs
{
    public class OrderBackgroundService : BackgroundService
    {
        private readonly ILogger<OrderBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentQueue<Order> _sendEmailQueue = new();
        private readonly ConcurrentQueue<Order> _cancelOrderQueue = new();

        public OrderBackgroundService(ILogger<OrderBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public void EnqueueSendEmail(Order order) => _sendEmailQueue.Enqueue(order);
        public void EnqueueCancelOrder(Order order) => _cancelOrderQueue.Enqueue(order);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderBackgroundService is running...");

            while (!stoppingToken.IsCancellationRequested)
            {
                while (_sendEmailQueue.TryDequeue(out var orderEmail))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var emailMessage = new EmailMessage
                    {
                        To = orderEmail.BuyerEmail,
                        Subject = $"Order #{orderEmail.Id} Confirmation",
                        HtmlBody = EmailTemplates.OrderConfirmation(orderEmail)
                    };

                    try
                    {
                        await emailService.SendAsync(emailMessage);
                        _logger.LogInformation("Email sent for Order {OrderId}", orderEmail.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email for Order {OrderId}", orderEmail.Id);
                    }
                }

                while (_cancelOrderQueue.TryDequeue(out var orderCancel))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                    try
                    {
                        await orderService.CancelOrder(orderCancel);
                        _logger.LogInformation("Order {OrderId} cancelled", orderCancel.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to cancel order {OrderId}", orderCancel.Id);
                    }
                }

                await Task.Delay(1000, stoppingToken); 
            }
        }
    }
}
