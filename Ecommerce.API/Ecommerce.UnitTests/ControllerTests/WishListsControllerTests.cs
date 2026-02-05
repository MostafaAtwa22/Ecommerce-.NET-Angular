using AutoMapper;
using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class WishListsControllerTests
    {
        private readonly Mock<IRedisRepository<CustomerWishList>> _redisRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly WishListsController _controller;

        public WishListsControllerTests()
        {
            _redisRepo = new Mock<IRedisRepository<CustomerWishList>>();
            _mapper = new Mock<IMapper>();
            _controller = new WishListsController(_redisRepo.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetById_ExistingWishList_ReturnsOkWithWishList()
        {
            // Arrange
            var wishListId = "wishlist123";
            var wishList = new CustomerWishList(wishListId)
            {
                Items = new List<WishListItem>
                {
                    new WishListItem { Id = 1, ProductName = "Product1", Price = 10.99m }
                }
            };

            _redisRepo.Setup(r => r.GetAsync(wishListId)).ReturnsAsync(wishList);

            // Act
            var result = await _controller.GetById(wishListId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnWishList = Assert.IsType<CustomerWishList>(okResult.Value);
            Assert.Equal(wishListId, returnWishList.Id);
            Assert.Single(returnWishList.Items);
        }

        [Fact]
        public async Task GetById_NonExistingWishList_ReturnsOkWithNewWishList()
        {
            // Arrange
            var wishListId = "wishlist123";
            _redisRepo.Setup(r => r.GetAsync(wishListId)).ReturnsAsync((CustomerWishList?)null);

            // Act
            var result = await _controller.GetById(wishListId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnWishList = Assert.IsType<CustomerWishList>(okResult.Value);
            Assert.Equal(wishListId, returnWishList.Id);
            Assert.Empty(returnWishList.Items);
        }

        [Fact]
        public async Task UpdateOrCreate_ValidDto_ReturnsOkWithWishList()
        {
            // Arrange
            var wishListDto = new CustomerWishListDto
            {
                Id = "wishlist123",
                Items = new List<WishListItemDto>
                {
                    new WishListItemDto { Id = 1, ProductName = "Product1", Price = 10.99m }
                }
            };

            var wishList = new CustomerWishList(wishListDto.Id)
            {
                Items = new List<WishListItem>
                {
                    new WishListItem { Id = 1, ProductName = "Product1", Price = 10.99m }
                }
            };

            _mapper.Setup(m => m.Map<CustomerWishList>(wishListDto)).Returns(wishList);
            _redisRepo.Setup(r => r.UpdateOrCreateAsync(wishListDto.Id, wishList, null)).ReturnsAsync(wishList);

            // Act
            var result = await _controller.UpdateOrCreate(wishListDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnWishList = Assert.IsType<CustomerWishList>(okResult.Value);
            Assert.Equal(wishListDto.Id, returnWishList.Id);
            _redisRepo.Verify(r => r.UpdateOrCreateAsync(wishListDto.Id, wishList, null), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingWishList_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            var wishListId = "wishlist123";
            _redisRepo.Setup(r => r.DeleteAsync(wishListId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(wishListId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("deleted successfully", okResult.Value?.ToString());
            _redisRepo.Verify(r => r.DeleteAsync(wishListId), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingWishList_ReturnsNotFound()
        {
            // Arrange
            var wishListId = "wishlist123";
            _redisRepo.Setup(r => r.DeleteAsync(wishListId)).ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(wishListId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFoundResult.Value?.ToString());
        }
    }
}
