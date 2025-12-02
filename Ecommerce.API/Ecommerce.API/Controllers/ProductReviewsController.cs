using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class ProductReviewsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductReviewsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductReviewsController(IUnitOfWork unitOfWork,
            ILogger<ProductReviewsController> logger,
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            IMapper mapper)
        {
            _logger = logger;
            _userManager = userManager;
            _productService = productService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ProductReviewDto>> Get([FromRoute] int productId)
        {
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .FindAllAsync(r => r.ProductId == productId);

            _logger.LogInformation($"You get all reviews for product with ID {productId}");

            return Ok(_mapper.Map<IEnumerable<ProductReviewDto>>(reviews));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<ProductReviewDto>>> Create([FromBody] ProductReviewCreationDto dto)
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);
            var userId = user!.Id;

            var existingReview = await _unitOfWork.Repository<ProductReview>()
                .FindAsync(r => r.ProductId == dto.ProductId && r.ApplicationUserId == userId);
            if (existingReview is not null)
                return BadRequest(new ApiResponse(400, "You have already reviewed this product."));

            var review = _mapper.Map<ProductReview>(dto);
            review.ApplicationUserId = userId;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Repository<ProductReview>().Create(review);
                await _productService.UpdateProductRatingAsync(dto.ProductId);
                await _unitOfWork.Complete();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResponse(400, "Failed to add review."));
            }

            var reviews = await _unitOfWork.Repository<ProductReview>()
                .FindAllAsync(r => r.ProductId == dto.ProductId);

            return Ok(_mapper.Map<IEnumerable<ProductReviewDto>>(reviews));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ProductReviewDto>> Delete(int id)
        {
            var review = await _unitOfWork.Repository<ProductReview>().GetByIdAsync(id);
            if (review == null)
                return NotFound(new ApiResponse(404, "Review not found."));

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Repository<ProductReview>().Delete(review);
                await _productService.UpdateProductRatingAsync(review.ProductId);
                await _unitOfWork.Complete();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResponse(400, "Failed to delete review."));
            }

            return Ok(_mapper.Map<ProductReviewDto>(review));
        }
    }
}