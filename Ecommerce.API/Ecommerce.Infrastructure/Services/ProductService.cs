
namespace Ecommerce.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<bool> UpdateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product is null)
            {
                _logger.LogError($"Product with ID: {productId} does not exist");
                return false;
            }

            var reviews = await _unitOfWork.Repository<ProductReview>().FindAllAsync(r => r.ProductId == productId);
            product.NumberOfReviews = reviews.Count;
            product.AverageRating = reviews.Count == 0
                ? 0
                : Math.Round(reviews.Average(r => r.Rating), 1);

            _unitOfWork.Repository<Product>().Update(product);
            var affected = await _unitOfWork.Complete();
            if (affected > 0) return true;

            _logger.LogError($"Failed to update product rating for product ID: {productId}");
            return false;
        }

    }
}
