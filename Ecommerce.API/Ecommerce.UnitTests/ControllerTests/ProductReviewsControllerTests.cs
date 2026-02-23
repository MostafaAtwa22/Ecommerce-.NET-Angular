using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class ProductReviewsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILogger<ProductReviewsController>> _logger;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<ProductReview>> _reviewRepo;
        private readonly ProductReviewsController _controller;

        public ProductReviewsControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _logger = new Mock<ILogger<ProductReviewsController>>();
            _productService = new Mock<IProductService>();
            _mapper = new Mock<IMapper>();
            _reviewRepo = new Mock<IGenericRepository<ProductReview>>();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _unitOfWork.Setup(u => u.Repository<ProductReview>()).Returns(_reviewRepo.Object);

            _controller = new ProductReviewsController(
                _unitOfWork.Object,
                _logger.Object,
                _userManager.Object,
                _productService.Object,
                _mapper.Object);

            // Setup HttpContext with user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.NameIdentifier, "user123")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task Get_ReturnsOkWithPaginatedReviews()
        {
            // Arrange
            var productId = 1;
            var specParams = new ProductReviewsSpecParams { PageIndex = 1, PageSize = 10 };
            var reviews = new List<ProductReview>
            {
                new ProductReview { Id = 1, ProductId = productId, Rating = 5, Comment = "Great!" },
                new ProductReview { Id = 2, ProductId = productId, Rating = 4, Comment = "Good" }
            };
            var reviewsDto = new List<ProductReviewDto>
            {
                new ProductReviewDto { Id = 1, Rating = 5, Comment = "Great!" },
                new ProductReviewDto { Id = 2, Rating = 4, Comment = "Good" }
            };

            _reviewRepo.Setup(r => r.CountAsync(It.IsAny<ISpecifications<ProductReview>>()))
                      .ReturnsAsync(2);
            _reviewRepo.Setup(r => r.GetAllWithSpecAsync(It.IsAny<ISpecifications<ProductReview>>()))
                      .ReturnsAsync(reviews);
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductReviewDto>>(reviews))
                  .Returns(reviewsDto);

            // Act
            var result = await _controller.Get(productId, specParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagination = Assert.IsType<Pagination<ProductReviewDto>>(okResult.Value);
            Assert.Equal(2, pagination.TotalData);
            Assert.Equal(2, pagination.Data.Count);
        }

        [Fact]
        public async Task Create_ValidReview_ReturnsOkWithReview()
        {
            // Arrange
            var dto = new ProductReviewFromDto { ProductId = 1, Rating = 5, Comment = "Great!" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = 1, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "user123" };
            var reviewDto = new ProductReviewDto { Id = 1, Rating = 5, Comment = "Great!" };
            var transaction = new Mock<IDbContextTransaction>();

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _reviewRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductReview, bool>>>()))
                      .ReturnsAsync((ProductReview?)null);
            _mapper.Setup(m => m.Map<ProductReview>(dto)).Returns(review);
            _unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<ProductReviewDto>(review)).Returns(reviewDto);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnReview = Assert.IsType<ProductReviewDto>(okResult.Value);
            Assert.Equal(1, returnReview.Id);
            _reviewRepo.Verify(r => r.Create(review), Times.Once);
            _productService.Verify(s => s.UpdateProductRatingAsync(dto.ProductId), Times.Once);
            transaction.Verify(t => t.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Create_DuplicateReview_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ProductReviewFromDto { ProductId = 1, Rating = 5, Comment = "Great!" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var existingReview = new ProductReview { Id = 1, ProductId = 1, ApplicationUserId = "user123" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _reviewRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductReview, bool>>>()))
                      .ReturnsAsync(existingReview);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("already reviewed", apiResponse.Message);
        }

        [Fact]
        public async Task Update_ValidReview_ReturnsOkWithUpdatedReview()
        {
            // Arrange
            var reviewId = 1;
            var dto = new ProductReviewFromDto { ProductId = 1, Rating = 4, Comment = "Updated" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "user123" };
            var reviewDto = new ProductReviewDto { Id = reviewId, Rating = 4, Comment = "Updated" };
            var transaction = new Mock<IDbContextTransaction>();

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
            _mapper.Setup(m => m.Map(dto, review)).Returns(review);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<ProductReviewDto>(review)).Returns(reviewDto);

            // Act
            var result = await _controller.Update(reviewId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnReview = Assert.IsType<ProductReviewDto>(okResult.Value);
            Assert.Equal(reviewId, returnReview.Id);
            _reviewRepo.Verify(r => r.Update(review), Times.Once);
            _productService.Verify(s => s.UpdateProductRatingAsync(review.ProductId), Times.Once);
            transaction.Verify(t => t.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Update_NonExistingReview_ReturnsNotFound()
        {
            // Arrange
            var reviewId = 1;
            var dto = new ProductReviewFromDto { ProductId = 1, Rating = 4, Comment = "Updated" };
            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((ProductReview?)null);

            // Act
            var result = await _controller.Update(reviewId, dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
            Assert.Contains("Review not found", apiResponse.Message);
        }

        [Fact]
        public async Task Update_UnauthorizedUser_ReturnsBadRequest()
        {
            // Arrange
            var reviewId = 1;
            var dto = new ProductReviewFromDto { ProductId = 1, Rating = 4, Comment = "Updated" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "otherUser" };

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));

            // Act
            var result = await _controller.Update(reviewId, dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("your own reviews only", apiResponse.Message);
        }

        [Fact]
        public async Task Delete_ValidReview_ReturnsNoContent()
        {
            // Arrange
            var reviewId = 1;
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "user123" };
            var transaction = new Mock<IDbContextTransaction>();

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(reviewId);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
            _reviewRepo.Verify(r => r.Delete(review), Times.Once);
            _productService.Verify(s => s.UpdateProductRatingAsync(review.ProductId), Times.Once);
            transaction.Verify(t => t.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingReview_ReturnsNotFound()
        {
            // Arrange
            var reviewId = 1;
            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((ProductReview?)null);

            // Act
            var result = await _controller.Delete(reviewId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
            Assert.Contains("Review not found", apiResponse.Message);
        }

        [Fact]
        public async Task Delete_UnauthorizedUser_ReturnsBadRequest()
        {
            // Arrange
            var reviewId = 1;
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "otherUser" };

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));

            // Act
            var result = await _controller.Delete(reviewId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("your own reviews only", apiResponse.Message);
        }

        [Fact]
        public async Task MarkHelpful_ValidReview_ReturnsOkWithUpdatedReview()
        {
            // Arrange
            var reviewId = 1;
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "otherUser", HelpfulCount = 0 };
            var reviewDto = new ProductReviewDto { Id = reviewId, Rating = 5, Comment = "Great!", HelpfulCount = 1 };

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _reviewRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductReview, bool>>>()))
                      .ReturnsAsync((ProductReview?)null);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _reviewRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<ProductReview>>()))
                      .ReturnsAsync(review);
            _mapper.Setup(m => m.Map<ProductReviewDto>(review)).Returns(reviewDto);

            // Act
            var result = await _controller.MarkHelpful(reviewId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnReview = Assert.IsType<ProductReviewDto>(okResult.Value);
            Assert.Equal(1, returnReview.HelpfulCount);
        }

        [Fact]
        public async Task MarkHelpful_OwnReview_ReturnsBadRequest()
        {
            // Arrange
            var reviewId = 1;
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "user123" };

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));

            // Act
            var result = await _controller.MarkHelpful(reviewId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("can't make feedback to your self", apiResponse.Message);
        }

        [Fact]
        public async Task MarkNotHelpful_ValidReview_ReturnsOkWithUpdatedReview()
        {
            // Arrange
            var reviewId = 1;
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };
            var review = new ProductReview { Id = reviewId, ProductId = 1, Rating = 5, Comment = "Great!", ApplicationUserId = "otherUser", NotHelpfulCount = 0 };
            var reviewDto = new ProductReviewDto { Id = reviewId, Rating = 5, Comment = "Great!", NotHelpfulCount = 1 };

            _reviewRepo.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _reviewRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductReview, bool>>>()))
                      .ReturnsAsync((ProductReview?)null);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _reviewRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<ProductReview>>()))
                      .ReturnsAsync(review);
            _mapper.Setup(m => m.Map<ProductReviewDto>(review)).Returns(reviewDto);

            // Act
            var result = await _controller.MarkNotHelpful(reviewId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnReview = Assert.IsType<ProductReviewDto>(okResult.Value);
            Assert.Equal(1, returnReview.NotHelpfulCount);
        }
    }
}
