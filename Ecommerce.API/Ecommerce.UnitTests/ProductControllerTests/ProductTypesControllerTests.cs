
namespace Ecommerce.UnitTests.ProductControllerTests
{
    public class ProductTypesControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<ProductType>> _typesRepo;
        private readonly ProductTypesController _controller;

        public ProductTypesControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _typesRepo = new Mock<IGenericRepository<ProductType>>();

            _unitOfWork.Setup(u => u.Repository<ProductType>())
                       .Returns(_typesRepo.Object);

            _controller = new ProductTypesController(_unitOfWork.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithTypes()
        {
            // Arrange
            var types = new List<ProductType>
            {
                new ProductType { Id = 1, Name = "Type1" },
                new ProductType { Id = 2, Name = "Type2" }
            };
            var typesDto = new List<ProductBrandAndTypeResponseDto>
            {
                new ProductBrandAndTypeResponseDto { Id = 1, Name = "Type1" },
                new ProductBrandAndTypeResponseDto { Id = 2, Name = "Type1" }
            };

            _typesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(types);
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductBrandAndTypeResponseDto>>(types))
                   .Returns(typesDto);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnTypes = Assert.IsAssignableFrom<IReadOnlyList<ProductBrandAndTypeResponseDto>>(okResult.Value);
            Assert.Equal(2, returnTypes.Count);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithType()
        {
            // Arrange
            var typeId = 1;
            var type = new ProductType { Id = typeId, Name = "Type1" };
            var typeDto = new ProductBrandAndTypeResponseDto { Id = typeId, Name = "Type1" };

            _typesRepo.Setup(r => r.GetByIdAsync(typeId)).ReturnsAsync(type);
            _mapper.Setup(m => m.Map<ProductBrandAndTypeResponseDto>(type))
                   .Returns(typeDto);

            // Act
            var result = await _controller.GetById(typeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnType = Assert.IsType<ProductBrandAndTypeResponseDto>(okResult.Value);
            Assert.Equal(typeId, returnType.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var typeId = 1;
            _typesRepo.Setup(r => r.GetByIdAsync(typeId)).ReturnsAsync((ProductType?)null);

            // Act
            var result = await _controller.GetById(typeId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreated()
        {
            // Arrange
            var creationDto = new ProductBrandAndTypeCreationDto { Name = "NewType" };
            var type = new ProductType { Id = 1, Name = "NewType" };
            var responseDto = new ProductBrandAndTypeResponseDto { Id = 1, Name = "NewType" };

            _mapper.Setup(m => m.Map<ProductBrandAndTypeCreationDto, ProductType>(creationDto))
                   .Returns(type);
            _typesRepo.Setup(r => r.Create(type)).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<ProductBrandAndTypeResponseDto>(type))
                   .Returns(responseDto);

            // Act
            var result = await _controller.Create(creationDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ProductTypesController.GetById), createdResult.ActionName);
            Assert.Equal(type.Id, createdResult.RouteValues!["id"]);
            var returnType = Assert.IsType<ProductBrandAndTypeResponseDto>(createdResult.Value);
            Assert.Equal("NewType", returnType.Name);

            _typesRepo.Verify(r => r.Create(type), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var typeId = 1;
            var type = new ProductType { Id = typeId, Name = "Type1" };

            _typesRepo.Setup(r => r.GetByIdAsync(typeId)).ReturnsAsync(type);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(typeId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _typesRepo.Verify(r => r.Delete(type), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var typeId = 1;
            _typesRepo.Setup(r => r.GetByIdAsync(typeId)).ReturnsAsync((ProductType?)null);

            // Act
            var result = await _controller.Delete(typeId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }
    }
}
