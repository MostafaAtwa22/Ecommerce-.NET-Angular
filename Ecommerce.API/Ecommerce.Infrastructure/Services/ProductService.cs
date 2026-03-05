using Microsoft.EntityFrameworkCore.Storage;
namespace Ecommerce.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, 
            ILogger<ProductService> logger, 
            IFileService fileService, 
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fileService = fileService;
            _mapper = mapper;
        }

        public async Task<bool> UpdateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product is null)
                throw new NotFoundException($"Product with ID {productId} not found");

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

        public async Task<Product> CreateProductAsync(ProductCreationDto creationDto)
        {
            var product = _mapper.Map<ProductCreationDto, Product>(creationDto);

            product.PictureUrl = await _fileService.SaveFileAsync(creationDto.ImageFile, "products");

            await _unitOfWork.Repository<Product>().Create(product);
            await _unitOfWork.Complete();

            return product;
        }

        public async Task<Product> UpdateProductAsync(ProductUpdateDto updateDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(updateDto.ProductId);

            if (product is null)
                throw new NotFoundException($"Product with ID {updateDto.ProductId} not found");

            var hasNewImage = updateDto.ImageFile is not null;
            var oldImage = product.PictureUrl;

            _mapper.Map(updateDto, product);

            if (hasNewImage)
                product.PictureUrl = await _fileService.SaveFileAsync(updateDto.ImageFile!, "products");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Repository<Product>().Update(product);
                await _unitOfWork.Complete();

                await transaction.CommitAsync();

                if (hasNewImage && !string.IsNullOrEmpty(oldImage))
                    _fileService.DeleteFile(oldImage);
            }
            catch
            {
                if (hasNewImage && !string.IsNullOrEmpty(product.PictureUrl))
                    _fileService.DeleteFile(product.PictureUrl);

                await transaction.RollbackAsync();
                throw;
            }

            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product is null)
                throw new NotFoundException($"Product with ID {id} not found");

            _unitOfWork.Repository<Product>().Delete(product);
            var affectedRows = await _unitOfWork.Complete();

            if (affectedRows > 0 && !string.IsNullOrEmpty(product.PictureUrl))
                _fileService.DeleteFile(product.PictureUrl);

            return affectedRows > 0;
        }
    }
}
