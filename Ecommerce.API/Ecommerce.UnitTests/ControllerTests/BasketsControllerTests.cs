
namespace Ecommerce.UnitTests.ControllerTests
{
    public class BasketsControllerTests
    {
        private readonly Mock<IRedisRepository<CustomerBasket>> _redisRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly BasketsController _controller;

        public BasketsControllerTests()
        {
            _redisRepo = new Mock<IRedisRepository<CustomerBasket>>();
            _mapper = new Mock<IMapper>();
            _controller = new BasketsController(_redisRepo.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetById_ExistingBasket_ReturnsOkWithBasket()
        {
            // Arrange
            var basketId = "basket123";
            var basket = new CustomerBasket(basketId)
            {
                Items = new List<BasketItem>
                {
                    new BasketItem { Id = 1, ProductName = "Product1", Price = 10.99m, Quantity = 2 }
                }
            };

            _redisRepo.Setup(r => r.GetAsync(basketId)).ReturnsAsync(basket);

            // Act
            var result = await _controller.GetById(basketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBasket = Assert.IsType<CustomerBasket>(okResult.Value);
            Assert.Equal(basketId, returnBasket.Id);
            Assert.Single(returnBasket.Items);
        }

        [Fact]
        public async Task GetById_NonExistingBasket_ReturnsOkWithNewBasket()
        {
            // Arrange
            var basketId = "basket123";
            _redisRepo.Setup(r => r.GetAsync(basketId)).ReturnsAsync((CustomerBasket?)null);

            // Act
            var result = await _controller.GetById(basketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBasket = Assert.IsType<CustomerBasket>(okResult.Value);
            Assert.Equal(basketId, returnBasket.Id);
            Assert.Empty(returnBasket.Items);
        }

        [Fact]
        public async Task UpdateOrCreate_ValidDto_ReturnsOkWithBasket()
        {
            // Arrange
            var basketDto = new CustomerBasketDto
            {
                Id = "basket123",
                Items = new List<BasketItemDto>
                {
                    new BasketItemDto { Id = 1, ProductName = "Product1", Price = 10.99m, Quantity = 2 }
                }
            };

            var basket = new CustomerBasket(basketDto.Id)
            {
                Items = new List<BasketItem>
                {
                    new BasketItem { Id = 1, ProductName = "Product1", Price = 10.99m, Quantity = 2 }
                }
            };

            _mapper.Setup(m => m.Map<CustomerBasket>(basketDto)).Returns(basket);
            _redisRepo.Setup(r => r.UpdateOrCreateAsync(basketDto.Id, basket, null)).ReturnsAsync(basket);

            // Act
            var result = await _controller.UpdateOrCreate(basketDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBasket = Assert.IsType<CustomerBasket>(okResult.Value);
            Assert.Equal(basketDto.Id, returnBasket.Id);
            _redisRepo.Verify(r => r.UpdateOrCreateAsync(basketDto.Id, basket, null), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingBasket_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var basketId = "basket123";
            _redisRepo.Setup(r => r.DeleteAsync(basketId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(basketId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("deleted successfully", okResult.Value?.ToString());
            _redisRepo.Verify(r => r.DeleteAsync(basketId), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingBasket_ReturnsNotFound()
        {
            // Arrange
            var basketId = "basket123";
            _redisRepo.Setup(r => r.DeleteAsync(basketId)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(basketId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFoundResult.Value?.ToString());
        }
    }
}
