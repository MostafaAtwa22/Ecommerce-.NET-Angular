using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Dtos;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.ProductControllerTests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<Product>> _productsRepo;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _productService = new Mock<IProductService>();
            _mapper = new Mock<IMapper>();
            _productsRepo = new Mock<IGenericRepository<Product>>();

            _unitOfWork.Setup(u => u.Repository<Product>())
                       .Returns(_productsRepo.Object);

            _controller = new ProductsController(_unitOfWork.Object, _productService.Object, _mapper.Object);
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
        public async Task Create_ValidDto_ReturnsOk()
        {
            // Arrange
            var creationDto = new ProductCreationDto { Name = "NewProduct", ImageFile = Mock.Of<IFormFile>() };
            var product = new Product { Id = 1, Name = "NewProduct" }; 
            var responseDto = new ProductResponseDto { Id = 1, Name = "NewProduct" };

            _productService.Setup(s => s.CreateProductAsync(creationDto))
                           .ReturnsAsync(product);
            
            _productsRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(product);

            _mapper.Setup(m => m.Map<Product, ProductResponseDto>(product))
                   .Returns(responseDto);

            // Act
            var result = await _controller.Create(creationDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProduct = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal("NewProduct", returnProduct.Name);
            
            _productService.Verify(s => s.CreateProductAsync(creationDto), Times.Once);
        }

        [Fact]
        public async Task Update_ValidDto_ReturnsOk()
        {
            // Arrange
            var productId = 1;
            var updateDto = new ProductUpdateDto { ProductId = productId, Name = "UpdatedProduct" };
            var product = new Product { Id = productId, Name = "UpdatedProduct" };
            var responseDto = new ProductResponseDto { Id = productId, Name = "UpdatedProduct" };

            _productService.Setup(s => s.UpdateProductAsync(updateDto))
                           .ReturnsAsync(product);
            
            _productsRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Product>>()))
                         .ReturnsAsync(product);

            _mapper.Setup(m => m.Map<Product, ProductResponseDto>(product))
                   .Returns(responseDto);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProduct = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal("UpdatedProduct", returnProduct.Name);

            _productService.Verify(s => s.UpdateProductAsync(updateDto), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;

            _productService.Setup(s => s.DeleteProductAsync(productId))
                           .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
            _productService.Verify(s => s.DeleteProductAsync(productId), Times.Once);
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
                new ProductSuggestionDto { Id = 1, Name = "Laptop" }
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
        }
    }
}
