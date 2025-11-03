namespace Ecommerce.Core.Entities
{
    public class CustomerBasket
    {
        public CustomerBasket()
        {
        }

        public CustomerBasket(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; } = string.Empty;

        public ICollection<BasketItem> Items { get; set; } = [];
    }
}