using Stripe;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ILogger<IPaymentService>> _logger;
        private readonly Mock<IConfiguration> _config;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _paymentService = new Mock<IPaymentService>();
            _logger = new Mock<ILogger<IPaymentService>>();
            _config = new Mock<IConfiguration>();
            _controller = new PaymentController(_paymentService.Object, _logger.Object, _config.Object);
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntent_ValidBasketId_ReturnsOkWithBasket()
        {
            // Arrange
            var basketId = "basket123";
            var basket = new CustomerBasket(basketId)
            {
                Items = new List<BasketItem>
                {
                    new BasketItem { Id = 1, ProductName = "Product1", Price = 10.99m, Quantity = 2 }
                },
                PaymentIntentId = "pi_123456"
            };

            _paymentService.Setup(s => s.CreateOrUpdatePaymentIntent(basketId)).ReturnsAsync(basket);

            // Act
            var result = await _controller.CreateOrUpdatePaymentIntent(basketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBasket = Assert.IsType<CustomerBasket>(okResult.Value);
            Assert.Equal(basketId, returnBasket.Id);
            Assert.Equal("pi_123456", returnBasket.PaymentIntentId);
            _paymentService.Verify(s => s.CreateOrUpdatePaymentIntent(basketId), Times.Once);
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntent_InvalidBasket_ReturnsBadRequest()
        {
            // Arrange
            var basketId = "basket123";
            _paymentService.Setup(s => s.CreateOrUpdatePaymentIntent(basketId)).ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _controller.CreateOrUpdatePaymentIntent(basketId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntent_EmptyBasket_ReturnsBadRequest()
        {
            // Arrange
            var basketId = "";
            _paymentService.Setup(s => s.CreateOrUpdatePaymentIntent(basketId)).ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _controller.CreateOrUpdatePaymentIntent(basketId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Equal(400, apiResponse.StatusCode);
        }

        [Fact]
        public async Task CreateOrUpdatePaymentIntent_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var basketId = "basket123";
            _paymentService.Setup(s => s.CreateOrUpdatePaymentIntent(basketId))
                          .ThrowsAsync(new Exception("Payment service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateOrUpdatePaymentIntent(basketId));
        }
    }
}
