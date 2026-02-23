namespace Ecommerce.Core.Entities
{
    public class CustomerBasket : IRedisEntity
    {
        public CustomerBasket()
        {
            Id = string.Empty;
        }

        public CustomerBasket(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
        public ICollection<BasketItem> Items { get; set; } = [];
        public int? DeliveryMethodId { get; set; }
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal shippingPrice { get; set; }
    }
}