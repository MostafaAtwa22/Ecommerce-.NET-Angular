namespace Ecommerce.API.Dtos.Requests
{
    public class CustomerWishListDto : IRedisDto
    {
        public string Id { get; set; } = string.Empty;

        public List<WishListItemDto> Items { get; set; } = new();
    }
}