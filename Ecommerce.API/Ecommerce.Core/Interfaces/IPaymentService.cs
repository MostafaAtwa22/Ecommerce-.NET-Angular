
namespace Ecommerce.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId, string? couponCode = null);
        Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId);
        Task<Order> UpdateOrderPaymentFailed(string paymentIntentId);
        Task RefundPaymentIntentAsync(string paymentIntentId, long? amountInCents, string reason);
    }
}
