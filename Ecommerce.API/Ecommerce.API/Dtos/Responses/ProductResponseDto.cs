namespace Ecommerce.API.Dtos.Responses
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PictureUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int BoughtQuantity { get; set; }

        public int NumberOfReviews { get; set; }

        public decimal AverageRating { get; set; }

        public int ProductBrandId { get; set; }
        
        public int ProductTypeId { get; set; }

        public string ProductBrandName { get; set; } = string.Empty;

        public string ProductTypeName { get; set; } = string.Empty;
    }
}