using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.Core.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string buyerEmail, string userId, int deliverMethodId, string basketId, OrderAddress shippingAddress);
        Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
        Task<Order?> GetOrderByIdAsync(int id, string buyerEmail);
        Task<bool> CancelOrder(ICollection<OrderItem> orderItems);
    }
}