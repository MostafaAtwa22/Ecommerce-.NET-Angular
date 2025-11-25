namespace Ecommerce.API.Dtos.Requests
{
    public class CustomerBasketDto : IRedisDto
    {
        public string Id { get; set; } = string.Empty;

        public ICollection<BasketItemDto> Items { get; set; } = new List<BasketItemDto>();
        public int? DeliveryMethodId { get; set; }
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal shippingPrice { get; set; }
    }
}