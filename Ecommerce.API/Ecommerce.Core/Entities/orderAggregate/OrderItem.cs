namespace Ecommerce.Core.Entities.orderAggregate
{
    public class OrderItem : BaseEntity
    {
        public OrderItem()
        {
        }

        public OrderItem(decimal price, int quantity, ProductItemOrdered productItemOrdered)
        {
            Price = price;
            Quantity = quantity;
            ProductItemOrdered = productItemOrdered;
        }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public ProductItemOrdered ProductItemOrdered { get; set; } = new();

        public int OrderId { get; set; }          
        public Order Order { get; set; } = null!;
    }
}