using AutoMapper;
using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.ProductControllerTests
{
    public class ProductBrandsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<ProductBrand>> _brandsRepo;
        private readonly ProductBrandsController _controller;

        public ProductBrandsControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _brandsRepo = new Mock<IGenericRepository<ProductBrand>>();

            _unitOfWork.Setup(u => u.Repository<ProductBrand>())
                       .Returns(_brandsRepo.Object);

            _controller = new ProductBrandsController(_unitOfWork.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithBrands()
        {
            // Arrange
            var brands = new List<ProductBrand>
            {
                new ProductBrand { Id = 1, Name = "Brand1" },
                new ProductBrand { Id = 2, Name = "Brand2" }
            };
            var brandsDto = new List<ProductBrandAndTypeResponseDto>
            {
                new ProductBrandAndTypeResponseDto { Id = 1, Name = "Brand1" },
                new ProductBrandAndTypeResponseDto { Id = 2, Name = "Brand2" }
            };

            _brandsRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(brands);
            _mapper.Setup(m => m.Map<IReadOnlyList<ProductBrandAndTypeResponseDto>>(brands))
                   .Returns(brandsDto);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBrands = Assert.IsAssignableFrom<IReadOnlyList<ProductBrandAndTypeResponseDto>>(okResult.Value);
            Assert.Equal(2, returnBrands.Count);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithBrand()
        {
            // Arrange
            var brandId = 1;
            var brand = new ProductBrand { Id = brandId, Name = "Brand1" };
            var brandDto = new ProductBrandAndTypeResponseDto { Id = brandId, Name = "Brand1" };

            _brandsRepo.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(brand);
            _mapper.Setup(m => m.Map<ProductBrandAndTypeResponseDto>(brand))
                   .Returns(brandDto);

            // Act
            var result = await _controller.GetById(brandId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBrand = Assert.IsType<ProductBrandAndTypeResponseDto>(okResult.Value);
            Assert.Equal(brandId, returnBrand.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var brandId = 1;
            _brandsRepo.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync((ProductBrand?)null);

            // Act
            var result = await _controller.GetById(brandId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreated()
        {
            // Arrange
            var creationDto = new ProductBrandAndTypeCreationDto { Name = "NewBrand" };
            var brand = new ProductBrand { Id = 1, Name = "NewBrand" };
            var responseDto = new ProductBrandAndTypeResponseDto { Id = 1, Name = "NewBrand" };

            _mapper.Setup(m => m.Map<ProductBrandAndTypeCreationDto, ProductBrand>(creationDto))
                   .Returns(brand);
            _brandsRepo.Setup(r => r.Create(brand)).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<ProductBrandAndTypeResponseDto>(brand))
                   .Returns(responseDto);

            // Act
            var result = await _controller.Create(creationDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ProductBrandsController.GetById), createdResult.ActionName);
            Assert.Equal(brand.Id, createdResult.RouteValues!["id"]);
            var returnBrand = Assert.IsType<ProductBrandAndTypeResponseDto>(createdResult.Value);
            Assert.Equal("NewBrand", returnBrand.Name);
            
            _brandsRepo.Verify(r => r.Create(brand), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var brandId = 1;
            var brand = new ProductBrand { Id = brandId, Name = "Brand1" };

            _brandsRepo.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync(brand);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(brandId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _brandsRepo.Verify(r => r.Delete(brand), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var brandId = 1;
            _brandsRepo.Setup(r => r.GetByIdAsync(brandId)).ReturnsAsync((ProductBrand?)null);

            // Act
            var result = await _controller.Delete(brandId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // The controller returns NotFound(new ApiResponse..) which wraps as NotFoundObjectResult
            Assert.IsType<ApiResponse>(notFoundResult.Value); 
        }
    }
}
