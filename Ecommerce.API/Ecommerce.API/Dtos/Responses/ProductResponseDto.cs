namespace Ecommerce.API.Dtos.Responses
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PictureUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string ProductBrandName { get; set; } = string.Empty;

        public string ProductTypeName { get; set; } = string.Empty;
    }
}