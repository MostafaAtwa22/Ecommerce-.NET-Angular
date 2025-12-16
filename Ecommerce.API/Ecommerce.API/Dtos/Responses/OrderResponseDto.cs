namespace Ecommerce.API.Dtos.Responses
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public DateTimeOffset OrderDate { get; set; } 
        public decimal ShippingPrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public IReadOnlyList<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();
        public OrderAddressDto AddressToShip { get; set; } = new();
        public string DeliveryMethod { get; set; } = string.Empty;
    }
}