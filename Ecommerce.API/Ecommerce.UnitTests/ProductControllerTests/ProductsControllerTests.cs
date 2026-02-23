
namespace Ecommerce.UnitTests.ProductControllerTests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IFileService> _fileService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<Product>> _productsRepo;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _fileService = new Mock<IFileService>();
            _mapper = new Mock<IMapper>();
            _productsRepo = new Mock<IGenericRepository<Product>>();

            _unitOfWork.Setup(u => u.Repository<Product>())
                       .Returns(_productsRepo.Object);

            _controller = new ProductsController(_unitOfWork.Object, _fileService.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithPagination()
        {
            // Arrange
            var specParams = new ProductSpecParams { PageIndex = 1, PageSize = 10 };
            var products = new List<Product> { new Product { Id = 1, Name = "Product1" } };
            var productDtos = new List<ProductResponseDto> { new ProductResponseDto { Id = 1, Name = "Product1" } };
            var totalCount = 1;

            _productsRepo.Setup(r => r.CountAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(totalCount);
            _productsRepo.Setup(r => r.GetAllWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(products);
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductResponseDto>>(products))
                   .Returns(productDtos);

            // Act
            var result = await _controller.GetAll(specParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagination = Assert.IsType<Pagination<ProductResponseDto>>(okResult.Value);
            
            Assert.Equal(specParams.PageIndex, pagination.PageIndex);
            Assert.Equal(specParams.PageSize, pagination.PageSize);
            Assert.Equal(totalCount, pagination.TotalData);
            Assert.Single(pagination.Data);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Product1" };
            var productDto = new ProductResponseDto { Id = productId, Name = "Product1" };

            _productsRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(product);
            _mapper.Setup(m => m.Map<Product, ProductResponseDto>(product))
                   .Returns(productDto);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProduct = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal(productId, returnProduct.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var productId = 1;
            _productsRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreated()
        {
            // Arrange
            var creationDto = new ProductCreationDto { Name = "NewProduct", ImageFile = Mock.Of<IFormFile>() };
            var product = new Product { Id = 0, Name = "NewProduct" }; // Id 0 initially
            var createdProduct = new Product { Id = 1, Name = "NewProduct", PictureUrl = "url/to/image" };
            var responseDto = new ProductResponseDto { Id = 1, Name = "NewProduct" };

            _mapper.Setup(m => m.Map<ProductCreationDto, Product>(creationDto))
                   .Returns(product);
            
            _fileService.Setup(f => f.SaveFileAsync(creationDto.ImageFile, "products"))
                        .ReturnsAsync("url/to/image");

            _productsRepo.Setup(r => r.Create(product)).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            
            // After save, repo is queried again for the created product using GetWithSpecAsync
            _productsRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(createdProduct);

            _mapper.Setup(m => m.Map<Product, ProductResponseDto>(createdProduct))
                   .Returns(responseDto);

            // Act
            var result = await _controller.Create(creationDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProduct = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal("NewProduct", returnProduct.Name);
            
            _productsRepo.Verify(r => r.Create(product), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task Update_ValidDto_ReturnsUpdated()
        {
            // Arrange
            var productId = 1;
            var updateDto = new ProductUpdateDto { ProductId = productId, Name = "UpdatedProduct" };
            var existingProduct = new Product { Id = productId, Name = "OriginalProduct", PictureUrl = "old/url" };
            var updatedProduct = new Product { Id = productId, Name = "UpdatedProduct", PictureUrl = "old/url" };
            var responseDto = new ProductResponseDto { Id = productId, Name = "UpdatedProduct" };

            _productsRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
            
            // Transaction mock
            var transaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);

            // Repo update
            _productsRepo.Setup(r => r.Update(existingProduct));
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            
            // Return updated
            _productsRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(updatedProduct);

            _mapper.Setup(m => m.Map<Product, ProductResponseDto>(updatedProduct))
                   .Returns(responseDto);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProduct = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal("UpdatedProduct", returnProduct.Name);

            _productsRepo.Verify(r => r.Update(existingProduct), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
            transaction.Verify(t => t.CommitAsync(default), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Product1", PictureUrl = "old/url" };

            _productsRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
            _productsRepo.Verify(r => r.Delete(product), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
            _fileService.Verify(f => f.DeleteFile(product.PictureUrl), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var productId = 1;
            _productsRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
             var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task GetSuggestions_EmptyTerm_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetSuggestions("   ", null, null, 8);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var suggestions = Assert.IsAssignableFrom<IReadOnlyList<ProductSuggestionDto>>(okResult.Value);
            Assert.Empty(suggestions);
            _productsRepo.Verify(r => r.GetAllWithSpecAsync(It.IsAny<ISpecifications<Product>>()), Times.Never);
        }

        [Fact]
        public async Task GetSuggestions_ValidTerm_ReturnsMappedSuggestions()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop" }
            };
            var suggestionDtos = new List<ProductSuggestionDto>
            {
                new ProductSuggestionDto { Id = 1, Name = "Laptop", ProductBrandName = "Dell", ProductTypeName = "Computers" }
            };

            _productsRepo.Setup(r => r.GetAllWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                .ReturnsAsync(products);
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductSuggestionDto>>(products))
                .Returns(suggestionDtos);

            // Act
            var result = await _controller.GetSuggestions("lap", null, null, 8);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var suggestions = Assert.IsAssignableFrom<IReadOnlyList<ProductSuggestionDto>>(okResult.Value);
            Assert.Single(suggestions);
            Assert.Equal("Laptop", suggestions[0].Name);
        }

        [Fact]
        public async Task GetSuggestions_WithBrandTypeFilters_RespectsFilters()
        {
            // Arrange
            _productsRepo.Setup(r => r.GetAllWithSpecAsync(It.Is<ISpecifications<Product>>(s =>
                    s.Criteria != null &&
                    s.Criteria.Compile()(new Product { Name = "Laptop", ProductBrandId = 1, ProductTypeId = 2 }) &&
                    !s.Criteria.Compile()(new Product { Name = "Laptop", ProductBrandId = 9, ProductTypeId = 2 }) &&
                    !s.Criteria.Compile()(new Product { Name = "Laptop", ProductBrandId = 1, ProductTypeId = 7 }) &&
                    !s.Criteria.Compile()(new Product { Name = "Phone", ProductBrandId = 1, ProductTypeId = 2 })
                )))
                .ReturnsAsync(new List<Product>());
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductSuggestionDto>>(It.IsAny<IReadOnlyList<Product>>()))
                .Returns(Array.Empty<ProductSuggestionDto>());

            // Act
            var result = await _controller.GetSuggestions("lap", 1, 2, 8);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var suggestions = Assert.IsAssignableFrom<IReadOnlyList<ProductSuggestionDto>>(okResult.Value);
            Assert.Empty(suggestions);
            _productsRepo.Verify(r => r.GetAllWithSpecAsync(It.IsAny<ISpecifications<Product>>()), Times.Once);
        }

        [Fact]
        public async Task GetSuggestions_LimitExceedsMax_ClampsToMax()
        {
            // Arrange
            _productsRepo.Setup(r => r.GetAllWithSpecAsync(It.Is<ISpecifications<Product>>(s => s.Take == 10)))
                .ReturnsAsync(new List<Product>());
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductSuggestionDto>>(It.IsAny<IReadOnlyList<Product>>()))
                .Returns(Array.Empty<ProductSuggestionDto>());

            // Act
            var result = await _controller.GetSuggestions("lap", null, null, 99);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var suggestions = Assert.IsAssignableFrom<IReadOnlyList<ProductSuggestionDto>>(okResult.Value);
            Assert.Empty(suggestions);
        }

        [Fact]
        public async Task GetSuggestions_NoMatches_ReturnsEmptyList()
        {
            // Arrange
            _productsRepo.Setup(r => r.GetAllWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                .ReturnsAsync(new List<Product>());
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductSuggestionDto>>(It.IsAny<IReadOnlyList<Product>>()))
                .Returns(Array.Empty<ProductSuggestionDto>());

            // Act
            var result = await _controller.GetSuggestions("nomatch", null, null, 8);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var suggestions = Assert.IsAssignableFrom<IReadOnlyList<ProductSuggestionDto>>(okResult.Value);
            Assert.Empty(suggestions);
        }
    }
}
