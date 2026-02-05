using System.Security.Claims;
using AutoMapper;
using Ecommerce.API.BackgroundJobs;
using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderService;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<OrderBackgroundService> _backgroundService;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IGenericRepository<Order>> _orderRepo;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _orderService = new Mock<IOrderService>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _backgroundService = new Mock<OrderBackgroundService>(
                new Mock<ILogger<OrderBackgroundService>>().Object,
                new Mock<IServiceScopeFactory>().Object
            );
            _mapper = new Mock<IMapper>();
            _orderRepo = new Mock<IGenericRepository<Order>>();

            _unitOfWork.Setup(u => u.Repository<Order>()).Returns(_orderRepo.Object);

            _controller = new OrdersController(
                _orderService.Object,
                _unitOfWork.Object,
                _backgroundService.Object,
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
        public async Task UpdateOrderStatus_ValidOrder_ReturnsOkWithUpdatedOrder()
        {
            // Arrange
            var orderId = 1;
            var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.PaymentReceived };
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                BuyerEmail = "test@example.com"
            };
            var orderDto = new OrderResponseDto { Id = orderId, Status = OrderStatus.PaymentReceived.ToString() };

            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync(order);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<Order, OrderResponseDto>(order)).Returns(orderDto);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrder = Assert.IsType<OrderResponseDto>(okResult.Value);
            Assert.Equal(orderId, returnOrder.Id);
            _orderRepo.Verify(r => r.Update(order), Times.Once);
            _unitOfWork.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_NonExistingOrder_ReturnsNotFound()
        {
            // Arrange
            var orderId = 1;
            var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.PaymentReceived };
            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync((Order?)null);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateOrderStatus_CompletedOrder_ReturnsBadRequest()
        {
            // Arrange
            var orderId = 1;
            var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.PaymentReceived };
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Complete,
                BuyerEmail = "test@example.com"
            };

            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync(order);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("Completed orders cannot be modified", apiResponse.Message);
        }

        [Fact]
        public async Task UpdateOrderStatus_CancelledOrder_ReturnsBadRequest()
        {
            // Arrange
            var orderId = 1;
            var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.PaymentReceived };
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Cancel,
                BuyerEmail = "test@example.com"
            };

            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync(order);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("Order already cancelled", apiResponse.Message);
        }

        [Fact]
        public async Task UpdateOrderStatus_CancelOrder_EnqueuesBackgroundJob()
        {
            // Arrange
            var orderId = 1;
            var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.Cancel };
            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                BuyerEmail = "test@example.com"
            };
            var orderDto = new OrderResponseDto { Id = orderId, Status = OrderStatus.Cancel.ToString() };

            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync(order);
            _unitOfWork.Setup(u => u.Complete()).ReturnsAsync(1);
            _mapper.Setup(m => m.Map<Order, OrderResponseDto>(order)).Returns(orderDto);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            _backgroundService.Verify(b => b.EnqueueCancelOrder(order), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ValidDto_ReturnsOkWithOrder()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                BasketId = "basket123",
                DeliveryMethodId = 1,
                ShipToAddress = new OrderAddressDto
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Street = "123 Main St",
                    City = "City",
                    Government = "State",
                    Zipcode = "12345"
                }
            };

            var orderAddress = new OrderAddress
            {
                FirstName = "John",
                LastName = "Doe",
                Street = "123 Main St",
                City = "City",
                Government = "State",
                Zipcode = "12345"
            };

            var order = new Order
            {
                Id = 1,
                BuyerEmail = "test@example.com",
                AddressToShip = orderAddress
            };

            var orderResponseDto = new OrderResponseDto { Id = 1, BuyerEmail = "test@example.com" };

            _mapper.Setup(m => m.Map<OrderAddressDto, OrderAddress>(orderDto.ShipToAddress))
                  .Returns(orderAddress);
            _orderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                orderDto.DeliveryMethodId,
                orderDto.BasketId,
                orderAddress))
                .ReturnsAsync(order);
            _mapper.Setup(m => m.Map<Order, OrderResponseDto>(order)).Returns(orderResponseDto);

            // Act
            var result = await _controller.CreateOrder(orderDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrder = Assert.IsType<OrderResponseDto>(okResult.Value);
            Assert.Equal(1, returnOrder.Id);
            _backgroundService.Verify(b => b.EnqueueSendEmail(order), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ServiceReturnsNull_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                BasketId = "basket123",
                DeliveryMethodId = 1,
                ShipToAddress = new OrderAddressDto
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Street = "123 Main St",
                    City = "City",
                    Government = "State",
                    Zipcode = "12345"
                }
            };

            var orderAddress = new OrderAddress();
            _mapper.Setup(m => m.Map<OrderAddressDto, OrderAddress>(orderDto.ShipToAddress))
                  .Returns(orderAddress);
            _orderService.Setup(s => s.CreateOrderAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                orderDto.DeliveryMethodId,
                orderDto.BasketId,
                orderAddress))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _controller.CreateOrder(orderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("Problem creating order", apiResponse.Message);
        }

        [Fact]
        public async Task GetOrderDetailsById_ExistingOrder_ReturnsOkWithOrder()
        {
            // Arrange
            var orderId = 1;
            var order = new Order
            {
                Id = orderId,
                BuyerEmail = "test@example.com"
            };
            var orderDto = new OrderResponseDto { Id = orderId, BuyerEmail = "test@example.com" };

            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync(order);
            _mapper.Setup(m => m.Map<Order, OrderResponseDto>(order)).Returns(orderDto);

            // Act
            var result = await _controller.GetOrderDetailsById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrder = Assert.IsType<OrderResponseDto>(okResult.Value);
            Assert.Equal(orderId, returnOrder.Id);
        }

        [Fact]
        public async Task GetOrderDetailsById_NonExistingOrder_ReturnsNotFound()
        {
            // Arrange
            var orderId = 1;
            _orderRepo.Setup(r => r.GetWithSpecAsync(It.IsAny<ISpecifications<Order>>()))
                     .ReturnsAsync((Order?)null);

            // Act
            var result = await _controller.GetOrderDetailsById(orderId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderById_ExistingOrder_ReturnsOkWithOrder()
        {
            // Arrange
            var orderId = 1;
            var order = new Order
            {
                Id = orderId,
                BuyerEmail = "test@example.com"
            };
            var orderDto = new OrderResponseDto { Id = orderId, BuyerEmail = "test@example.com" };

            _orderService.Setup(s => s.GetOrderByIdAsync(orderId, It.IsAny<string>()))
                        .ReturnsAsync(order);
            _mapper.Setup(m => m.Map<Order, OrderResponseDto>(order)).Returns(orderDto);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrder = Assert.IsType<OrderResponseDto>(okResult.Value);
            Assert.Equal(orderId, returnOrder.Id);
        }

        [Fact]
        public async Task GetOrderById_NonExistingOrder_ReturnsNotFound()
        {
            // Arrange
            var orderId = 1;
            _orderService.Setup(s => s.GetOrderByIdAsync(orderId, It.IsAny<string>()))
                        .ReturnsAsync((Order?)null);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(notFoundResult.Value);
        }

        [Fact]
        public async Task GetOrders_ReturnsOkWithOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, BuyerEmail = "test@example.com" },
                new Order { Id = 2, BuyerEmail = "test@example.com" }
            };
            var ordersDto = new List<OrderResponseDto>
            {
                new OrderResponseDto { Id = 1, BuyerEmail = "test@example.com" },
                new OrderResponseDto { Id = 2, BuyerEmail = "test@example.com" }
            };

            _orderService.Setup(s => s.GetOrdersForUserAsync(It.IsAny<string>()))
                        .ReturnsAsync(orders);
            _mapper.Setup(m => m.Map<IReadOnlyList<Order>, IReadOnlyList<OrderResponseDto>>(orders))
                  .Returns(ordersDto);

            // Act
            var result = await _controller.GetOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnOrders = Assert.IsAssignableFrom<IReadOnlyList<OrderResponseDto>>(okResult.Value);
            Assert.Equal(2, returnOrders.Count);
        }
    }
}
