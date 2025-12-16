using Ecommerce.Core.Entities.Identity;

namespace Ecommerce.Core.Entities.orderAggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {
        }

        public Order(ICollection<OrderItem> orderItems,
            string buyerEmail,
            decimal subTotal,
            OrderAddress addressToShip,
            DeliveryMethod deliveryMethod)
        {
            OrderItems = orderItems;
            BuyerEmail = buyerEmail;
            SubTotal = subTotal;
            DeliveryMethod = deliveryMethod;
            AddressToShip = addressToShip;
        }

        public string BuyerEmail { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
        public decimal SubTotal { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public OrderAddress AddressToShip { get; set; } = new();
        public DeliveryMethod DeliveryMethod { get; set; } = new();
        public string PaymentIntenId { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = default!;
        
        public decimal GetTotal()
            => SubTotal + DeliveryMethod.Price;
    }
}