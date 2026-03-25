namespace Ecommerce.Core.Interfaces
{
    public interface IProductService
    {
        Task<bool> UpdateProductRatingAsync(int productId);
        Task<Product> CreateProductAsync(ProductCreationDto creationDto);
        Task<Product> UpdateProductAsync(ProductUpdateDto updateDto);
        Task<bool> DeleteProductAsync(int id);
        Task CleanExpiredDiscountsAsync();
    }
}