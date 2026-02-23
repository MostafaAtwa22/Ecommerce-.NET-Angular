namespace Ecommerce.API.Dtos.Responses
{
    public class ProductSuggestionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProductBrandName { get; set; } = string.Empty;
        public string ProductTypeName { get; set; } = string.Empty;
    }
}
