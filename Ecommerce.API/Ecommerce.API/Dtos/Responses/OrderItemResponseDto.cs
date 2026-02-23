namespace Ecommerce.API.Dtos.Responses
{
    public class OrderItemResponseDto
    {
        public int ProductItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}