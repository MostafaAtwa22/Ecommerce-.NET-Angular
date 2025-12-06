using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

        // [Cached(600)]
        [HttpGet("{productId}")]
        public async Task<ActionResult<Pagination<ProductReviewDto>>> Get(int productId, [FromQuery] ProductReviewsSpecParams specParams)
        {
            var spec = new ProductReviewsWithApplicationUser(productId, specParams, false);
            var countSpec = new ProductReviewsWithApplicationUser(productId, specParams, true);

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

        [Authorize(Roles = "Customer")]
        [HttpPost]
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

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}")]
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

        [Authorize(Roles = "Customer")]
        [HttpDelete("{id}")]
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

            var spec = new ProductReviewsWithApplicationUser(review.Id);
            var reviewWithSpec = await _unitOfWork.Repository<ProductReview>()
                .GetWithSpecAsync(spec);
            return Ok(_mapper.Map<ProductReviewDto>(reviewWithSpec));
        }

        [HttpPost("{id}/not-helpful")]
        [Authorize] 
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

            var spec = new ProductReviewsWithApplicationUser(review.Id); 
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