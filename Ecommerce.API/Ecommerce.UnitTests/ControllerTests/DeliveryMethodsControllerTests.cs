using AutoMapper;
using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class DeliveryMethodsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<DeliveryMethod>> _deliveryMethodRepo;
        private readonly DeliveryMethodsController _controller;

        public DeliveryMethodsControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mapper = new Mock<IMapper>();
            _deliveryMethodRepo = new Mock<IGenericRepository<DeliveryMethod>>();

            _unitOfWork.Setup(u => u.Repository<DeliveryMethod>())
                       .Returns(_deliveryMethodRepo.Object);

            _controller = new DeliveryMethodsController(_unitOfWork.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetDeliveryMethods_ReturnsOkWithDeliveryMethods()
        {
            // Arrange
            var deliveryMethods = new List<DeliveryMethod>
            {
                new DeliveryMethod { Id = 1, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m },
                new DeliveryMethod { Id = 2, ShortName = "Express", DeliveryTime = "1-2 days", Description = "Express delivery", Price = 15.00m }
            };
            var deliveryMethodsDto = new List<DeliveryMethodResponseDto>
            {
                new DeliveryMethodResponseDto { Id = 1, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m },
                new DeliveryMethodResponseDto { Id = 2, ShortName = "Express", DeliveryTime = "1-2 days", Description = "Express delivery", Price = 15.00m }
            };

            _deliveryMethodRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(deliveryMethods);
            _mapper.Setup(m => m.Map<IReadOnlyList<DeliveryMethodResponseDto>>(It.IsAny<List<DeliveryMethod>>()))
                   .Returns(deliveryMethodsDto);

            // Act
            var result = await _controller.GetDeliveryMethods();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnMethods = Assert.IsAssignableFrom<IReadOnlyList<DeliveryMethodResponseDto>>(okResult.Value);
            Assert.Equal(2, returnMethods.Count);
        }

        [Fact]
        public async Task GetDeliveryMethod_ExistingId_ReturnsOkWithDeliveryMethod()
        {
            // Arrange
            var methodId = 1;
            var deliveryMethod = new DeliveryMethod { Id = methodId, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };
            var deliveryMethodDto = new DeliveryMethodResponseDto { Id = methodId, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };

            _deliveryMethodRepo.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(deliveryMethod);
            _mapper.Setup(m => m.Map<DeliveryMethodResponseDto>(deliveryMethod))
                   .Returns(deliveryMethodDto);

            // Act
            var result = await _controller.GetDeliveryMethod(methodId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnMethod = Assert.IsType<DeliveryMethodResponseDto>(okResult.Value);
            Assert.Equal(methodId, returnMethod.Id);
        }

        [Fact]
        public async Task GetDeliveryMethod_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var methodId = 1;
            _deliveryMethodRepo.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _controller.GetDeliveryMethod(methodId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task CreateDeliveryMethod_ValidDto_ReturnsCreated()
        {
            // Arrange
            var createDto = new DeliveryMethodDto { ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };
            var deliveryMethod = new DeliveryMethod { Id = 1, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };
            var responseDto = new DeliveryMethodResponseDto { Id = 1, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };

            _mapper.Setup(m => m.Map<DeliveryMethod>(createDto)).Returns(deliveryMethod);
            _deliveryMethodRepo.Setup(r => r.Create(deliveryMethod)).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<DeliveryMethodResponseDto>(deliveryMethod)).Returns(responseDto);

            // Act
            var result = await _controller.CreateDeliveryMethod(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(DeliveryMethodsController.GetDeliveryMethod), createdResult.ActionName);
            Assert.Equal(deliveryMethod.Id, createdResult.RouteValues!["id"]);
            var returnMethod = Assert.IsType<DeliveryMethodResponseDto>(createdResult.Value);
            Assert.Equal("Standard", returnMethod.ShortName);

            _deliveryMethodRepo.Verify(r => r.Create(deliveryMethod), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task UpdateDeliveryMethod_ExistingId_ReturnsOkWithUpdatedMethod()
        {
            // Arrange
            var methodId = 1;
            var updateDto = new DeliveryMethodDto { ShortName = "Updated", DeliveryTime = "3-5 days", Description = "Updated delivery", Price = 10.00m };
            var deliveryMethod = new DeliveryMethod { Id = methodId, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };
            var responseDto = new DeliveryMethodResponseDto { Id = methodId, ShortName = "Updated", DeliveryTime = "3-5 days", Description = "Updated delivery", Price = 10.00m };

            _deliveryMethodRepo.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(deliveryMethod);
            _mapper.Setup(m => m.Map(updateDto, deliveryMethod)).Returns(deliveryMethod);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<DeliveryMethodResponseDto>(deliveryMethod)).Returns(responseDto);

            // Act
            var result = await _controller.UpdateDeliveryMethod(methodId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnMethod = Assert.IsType<DeliveryMethodResponseDto>(okResult.Value);
            Assert.Equal(methodId, returnMethod.Id);

            _deliveryMethodRepo.Verify(r => r.Update(deliveryMethod), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task UpdateDeliveryMethod_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var methodId = 1;
            var updateDto = new DeliveryMethodDto { ShortName = "Updated", DeliveryTime = "3-5 days", Description = "Updated delivery", Price = 10.00m };
            _deliveryMethodRepo.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _controller.UpdateDeliveryMethod(methodId, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteDeliveryMethod_ExistingId_ReturnsOkWithDeletedMethod()
        {
            // Arrange
            var methodId = 1;
            var deliveryMethod = new DeliveryMethod { Id = methodId, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };
            var responseDto = new DeliveryMethodResponseDto { Id = methodId, ShortName = "Standard", DeliveryTime = "5-7 days", Description = "Standard delivery", Price = 5.00m };

            _deliveryMethodRepo.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(deliveryMethod);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<DeliveryMethodResponseDto>(deliveryMethod)).Returns(responseDto);

            // Act
            var result = await _controller.DeleteDeliveryMethod(methodId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnMethod = Assert.IsType<DeliveryMethodResponseDto>(okResult.Value);
            Assert.Equal(methodId, returnMethod.Id);

            _deliveryMethodRepo.Verify(r => r.Delete(deliveryMethod), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task DeleteDeliveryMethod_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var methodId = 1;
            _deliveryMethodRepo.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync((DeliveryMethod?)null);

            // Act
            var result = await _controller.DeleteDeliveryMethod(methodId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }
    }
}
