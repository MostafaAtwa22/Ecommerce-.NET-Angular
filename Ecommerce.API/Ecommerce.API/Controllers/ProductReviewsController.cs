
namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-browsing")]
    public class ProductReviewsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductReviewsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductReviewsController(
            IUnitOfWork unitOfWork,
            ILogger<ProductReviewsController> logger,
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _productService = productService;
            _mapper = mapper;
        }

        [Cached(200)]
        [HttpGet("{productId}")]
        public async Task<ActionResult<Pagination<ProductReviewDto>>> Get(int productId, [FromQuery] ProductReviewsSpecParams specParams)
        {
            var spec = ProductReviewSpecifications.BuildListingSpec(productId, specParams);
            var countSpec = ProductReviewSpecifications.BuildListingCountSpec(productId, specParams);

            var totalItems = await _unitOfWork.Repository<ProductReview>()
                .CountAsync(countSpec);

            var reviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecAsync(spec);

            _logger.LogInformation("Retrieved {Count} reviews for product {ProductId}", reviews.Count(), productId);

            var data = _mapper.Map<IReadOnlyList<ProductReviewDto>>(reviews);

            return Ok(new Pagination<ProductReviewDto>(
                specParams.PageIndex,
                specParams.PageSize,
                totalItems,
                data
            ));
        }

        [HttpPost]
        [InvalidateCache("/api/productReviews")]
        [AuthorizePermission(Modules.ProductReviews, CRUD.Create)]
        public async Task<ActionResult<ProductReviewDto>> Create(ProductReviewFromDto dto)
        {
            var user = await GetCurrentUserAsync();
            var userId = user.Id;

            var existingReview = await _unitOfWork.Repository<ProductReview>()
                .FindAsync(r => r.ProductId == dto.ProductId && r.ApplicationUserId == userId);

            if (existingReview != null)
            {
                return BadRequest(new ApiResponse(400, "You have already reviewed this product."));
            }

            var review = _mapper.Map<ProductReview>(dto);
            review.ApplicationUserId = userId;

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Repository<ProductReview>().Create(review);
                await _unitOfWork.Complete();

                await _productService.UpdateProductRatingAsync(dto.ProductId);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create review");
                return BadRequest(new ApiResponse(400, "Failed to add review."));
            }

            return Ok(_mapper.Map<ProductReviewDto>(review));
        }

        [HttpPut("{id}")]
        [AuthorizePermission(Modules.ProductReviews, CRUD.Update)]
        [InvalidateCache("/api/productReviews")]
        public async Task<ActionResult<ProductReviewDto>> Update(int id, ProductReviewFromDto dto)
        {
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(id);
            if (review == null)
                return NotFound(new ApiResponse(404, "Review not found."));

            var user = await GetCurrentUserAsync();

            if (review.ApplicationUserId != user.Id)
                return BadRequest(new ApiResponse(400, "You can update your own reviews only."));

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                _mapper.Map(dto, review);
                _unitOfWork.Repository<ProductReview>().Update(review);
                await _unitOfWork.Complete();

                await _productService.UpdateProductRatingAsync(review.ProductId);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update review");
                return BadRequest(new ApiResponse(400, "Failed to update review."));
            }

            return Ok(_mapper.Map<ProductReviewDto>(review));
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(Modules.ProductReviews, CRUD.Delete)]
        [InvalidateCache("/api/productReviews")]
        public async Task<ActionResult<ProductReviewDto>> Delete(int id)
        {
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(id);
            if (review == null)
                return NotFound(new ApiResponse(404, "Review not found."));

            var user = await GetCurrentUserAsync();

            if (review.ApplicationUserId != user.Id)
                return BadRequest(new ApiResponse(400, "You can delete your own reviews only."));

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                _unitOfWork.Repository<ProductReview>().Delete(review);
                await _unitOfWork.Complete();

                await _productService.UpdateProductRatingAsync(review.ProductId);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete review");
                return BadRequest(new ApiResponse(400, "Failed to delete review."));
            }

            return NoContent();
        }

        [HttpPost("{id}/helpful")]
        [Authorize]
        [AuthorizePermission(Modules.ProductReviews, CRUD.Create)]
        [InvalidateCache("/api/productReviews")]
        public async Task<ActionResult> MarkHelpful(int id)
        {
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(id);
            if (review is null)
                return NotFound(new ApiResponse(404, "Review not found."));
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            if (review.ApplicationUserId == user!.Id)
                return BadRequest(new ApiResponse(400, "You can't make feedback to your self"));

            review.HelpfulCount++;
            await _unitOfWork.Complete();

            var spec = ProductReviewSpecifications.BuildDetailsSpec(review.Id);
            var reviewWithSpec = await _unitOfWork.Repository<ProductReview>()
                .GetWithSpecAsync(spec);
            return Ok(_mapper.Map<ProductReviewDto>(reviewWithSpec));
        }

        [HttpPost("{id}/not-helpful")]
        [Authorize] 
        [AuthorizePermission(Modules.ProductReviews, CRUD.Create)]
        [InvalidateCache("/api/productReviews")]
        public async Task<ActionResult> MarkNotHelpful(int id) 
        { 
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(id); 
            if (review is null) 
                return NotFound(new ApiResponse(404, "Review not found.")); 
            
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User); 
            if (review.ApplicationUserId == user!.Id) 
                return BadRequest(new ApiResponse(400, "You can't make feedback to your self")); 
    
            review.NotHelpfulCount++; 
            await _unitOfWork.Complete(); 

            var spec = ProductReviewSpecifications.BuildDetailsSpec(review.Id); 
            var reviewWithSpec = await _unitOfWork.Repository<ProductReview>()
                .GetWithSpecAsync(spec); 
        
            return Ok(_mapper.Map<ProductReviewDto>(reviewWithSpec)); 
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User)
                    ?? throw new Exception("User not found");
        }
    }
}
