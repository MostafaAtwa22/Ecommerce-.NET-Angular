namespace Ecommerce.Core.Entities
{
    public class CustomerWishList : IRedisEntity
    {
        public CustomerWishList()
        {
            Id = string.Empty;
        }

        public CustomerWishList(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
        public List<WishListItem> Items { get; set; } = new();
    }
}