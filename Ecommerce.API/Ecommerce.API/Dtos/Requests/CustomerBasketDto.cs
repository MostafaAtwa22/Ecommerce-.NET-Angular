namespace Ecommerce.API.Dtos.Requests
{
    public class CustomerBasketDto
    {
        public string Id { get; set; } = string.Empty;

        public ICollection<BasketItemDto> Items { get; set; } = new List<BasketItemDto>();
    }
}