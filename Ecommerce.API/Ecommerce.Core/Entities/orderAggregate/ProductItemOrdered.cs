namespace Ecommerce.Core.Entities.orderAggregate
{
    public class ProductItemOrdered
    {
        public ProductItemOrdered()
        {
        }

        public ProductItemOrdered(int productItemId, string productName, string pictureUrl, decimal discountPercentage = 0)
        {
            ProductItemId = productItemId;
            ProductName = productName;
            PictureUrl = pictureUrl;
            DiscountPercentage = discountPercentage;
        }

        public int ProductItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; } = 0;
    }
}