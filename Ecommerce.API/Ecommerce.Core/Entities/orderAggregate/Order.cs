namespace Ecommerce.Core.Entities.orderAggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {
        }

        public Order(IReadOnlyList<OrderItem> orderItems,
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
        public IReadOnlyList<OrderItem> OrderItems { get; set; } = [];
        public OrderAddress AddressToShip { get; set; } = new();
        public DeliveryMethod DeliveryMethod { get; set; } = new();
        public string PaymentIntenId { get; set; } = string.Empty;

        public decimal GetTotal()
            => SubTotal + DeliveryMethod.Price;
    }
}