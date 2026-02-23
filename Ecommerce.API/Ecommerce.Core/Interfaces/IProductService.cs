namespace Ecommerce.Core.Interfaces
{
    public interface IProductService
    {
        Task<bool> UpdateProductRatingAsync(int productId);
    }
}