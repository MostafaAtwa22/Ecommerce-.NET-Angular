using System.Security.Claims;
using AutoMapper;
using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ecommerce.API.Extensions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Ecommerce.API.Extensions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Ecommerce.UnitTests.Helpers;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class ProfilesControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<IFileService> _fileService;
        private readonly Mock<ILogger<ProfilesController>> _logger;
        private readonly Mock<IMapper> _mapper;
        private readonly ProfilesController _controller;

        public ProfilesControllerTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);
            _fileService = new Mock<IFileService>();
            _logger = new Mock<ILogger<ProfilesController>>();
            _mapper = new Mock<IMapper>();

            _controller = new ProfilesController(
                _userManager.Object,
                _fileService.Object,
                _logger.Object,
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
        public async Task GetProfile_ReturnsOkWithProfile()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com", FirstName = "Test", LastName = "User" };
            var profileDto = new ProfileResponseDto { Email = "test@example.com", FirstName = "Test", LastName = "User" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _mapper.Setup(m => m.Map<ProfileResponseDto>(user)).Returns(profileDto);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProfile = Assert.IsType<ProfileResponseDto>(okResult.Value);
            Assert.Equal("test@example.com", returnProfile.Email);
        }

        [Fact]
        public async Task GetAddress_ReturnsOkWithAddress()
        {
            // Arrange
            var address = new Address { Street = "123 Main St", City = "New York", Government = "NY", Zipcode = "10001", Country = "USA" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com", Address = address };
            var addressDto = new AddressDto { Street = "123 Main St", City = "New York", Government = "NY", Zipcode = "10001", Country = "USA" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _mapper.Setup(m => m.Map<Address, AddressDto>(address)).Returns(addressDto);

            // Act
            var result = await _controller.GetAddress();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnAddress = Assert.IsType<AddressDto>(okResult.Value);
            Assert.Equal("123 Main St", returnAddress.Street);
        }

        [Fact]
        public async Task UpdateAddress_ValidDto_ReturnsOkWithUpdatedAddress()
        {
            // Arrange
            var addressDto = new AddressDto { Street = "456 Oak St", City = "LA", Government = "CA", Zipcode = "90001", Country = "USA" };
            var address = new Address { Street = "456 Oak St", City = "LA", Government = "CA", Zipcode = "90001", Country = "USA" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _mapper.Setup(m => m.Map<AddressDto, Address>(addressDto)).Returns(address);
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mapper.Setup(m => m.Map<Address, AddressDto>(address)).Returns(addressDto);

            // Act
            var result = await _controller.UpdateAddress(addressDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnAddress = Assert.IsType<AddressDto>(okResult.Value);
            Assert.Equal("456 Oak St", returnAddress.Street);
            _userManager.Verify(u => u.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAddress_UpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var addressDto = new AddressDto { Street = "456 Oak St", City = "LA", Government = "CA", Zipcode = "90001", Country = "USA" };
            var address = new Address { Street = "456 Oak St", City = "LA", Government = "CA", Zipcode = "90001", Country = "USA" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _mapper.Setup(m => m.Map<AddressDto, Address>(addressDto)).Returns(address);
            _userManager.Setup(u => u.UpdateAsync(user))
                       .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            var result = await _controller.UpdateAddress(addressDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(badRequestResult.Value);
        }

        [Fact]
        public async Task ChangePassword_ValidDto_ReturnsOkTrue()
        {
            // Arrange
            var dto = new ChangePassowrdDto { OldPassword = "OldPass123!", NewPassword = "NewPass123!" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword))
                       .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
            _userManager.Verify(u => u.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ChangePassowrdDto { OldPassword = "WrongPass", NewPassword = "NewPass123!" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword))
                       .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<ApiResponse>(badRequestResult.Value);
        }

        [Fact]
        public async Task SetPassword_ValidDto_ReturnsOkTrue()
        {
            // Arrange
            var dto = new SetPasswordDto { Password = "NewPass123!" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.AddPasswordAsync(user, dto.Password))
                       .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.SetPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
            _userManager.Verify(u => u.AddPasswordAsync(user, dto.Password), Times.Once);
        }

        [Fact]
        public async Task DeleteProfile_ValidPassword_ReturnsOkTrue()
        {
            // Arrange
            var dto = new DeleteProfileDto { Password = "Pass123!" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com", ProfilePictureUrl = "profile.jpg" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.CheckPasswordAsync(user, dto.Password))
                       .ReturnsAsync(true);
            _userManager.Setup(u => u.DeleteAsync(user))
                       .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteProfile(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
            _userManager.Verify(u => u.DeleteAsync(user), Times.Once);
            _fileService.Verify(f => f.DeleteFile(user.ProfilePictureUrl), Times.Once);
        }

        [Fact]
        public async Task DeleteProfile_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = new DeleteProfileDto { Password = "WrongPass" };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.CheckPasswordAsync(user, dto.Password))
                       .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProfile(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("Incorrect password", apiResponse.Message);
        }

        [Fact]
        public async Task LockUser_ValidUser_ReturnsOkWithProfile()
        {
            // Arrange
            var userId = "user123";
            var user = new ApplicationUser { Id = userId, Email = "test@example.com", LockoutEnabled = false };
            var profileDto = new ProfileResponseDto { Email = "test@example.com" };

            _userManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
                       .ReturnsAsync(IdentityResult.Success);
            _mapper.Setup(m => m.Map<ProfileResponseDto>(user)).Returns(profileDto);

            // Act
            var result = await _controller.LockUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProfile = Assert.IsType<ProfileResponseDto>(okResult.Value);
            _userManager.Verify(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()), Times.Once);
        }

        [Fact]
        public async Task LockUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "nonexistent";
            _userManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.LockUser(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UnlockUser_ValidUser_ReturnsOkWithProfile()
        {
            // Arrange
            var userId = "user123";
            var user = new ApplicationUser { Id = userId, Email = "test@example.com", LockoutEnabled = true };
            var profileDto = new ProfileResponseDto { Email = "test@example.com" };

            _userManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManager.Setup(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
                       .ReturnsAsync(IdentityResult.Success);
            _mapper.Setup(m => m.Map<ProfileResponseDto>(user)).Returns(profileDto);

            // Act
            var result = await _controller.UnlockUserAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnProfile = Assert.IsType<ProfileResponseDto>(okResult.Value);
            _userManager.Verify(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()), Times.Once);
        }

        [Fact]
        public async Task Toggle2FA_EnableTwoFactor_ReturnsOkWithMessage()
        {
            // Arrange
            var dto = new Toggle2FADto { Enable = true };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.SetTwoFactorEnabledAsync(user, dto.Enable))
                       .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Toggle2FA(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("enabled successfully", okResult.Value?.ToString());
            _userManager.Verify(u => u.SetTwoFactorEnabledAsync(user, true), Times.Once);
        }

        [Fact]
        public async Task Toggle2FA_DisableTwoFactor_ReturnsOkWithMessage()
        {
            // Arrange
            var dto = new Toggle2FADto { Enable = false };
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com" };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));
            _userManager.Setup(u => u.SetTwoFactorEnabledAsync(user, dto.Enable))
                       .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Toggle2FA(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("disabled successfully", okResult.Value?.ToString());
            _userManager.Verify(u => u.SetTwoFactorEnabledAsync(user, false), Times.Once);
        }

        [Fact]
        public async Task Get2FAStatus_ReturnsOkWithStatus()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user123", Email = "test@example.com", TwoFactorEnabled = true };

            var users = new List<ApplicationUser> { user };
            _userManager.Setup(u => u.Users).Returns(new TestAsyncEnumerable<ApplicationUser>(users));

            // Act
            var result = await _controller.Get2FAStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)okResult.Value!);
        }
    }


}
