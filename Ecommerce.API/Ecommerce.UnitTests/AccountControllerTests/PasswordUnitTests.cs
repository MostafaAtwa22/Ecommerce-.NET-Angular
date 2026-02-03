using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class PasswordUnitTests : AccountControllerTestsBase
    {
        public PasswordUnitTests() : base()
        {
        }

        [Fact]
        [Trait("AccountController", "ForgetPassword")]
        public async Task ForgetPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ForgetPasswordDto { Email = "test@test.com" };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.ForgetPassword(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "ForgetPassword")]
        public async Task ForgetPassword_Success_ReturnsOk()
        {
            // Arrange
            var dto = new ForgetPasswordDto { Email = "test@test.com" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                        .ReturnsAsync("reset-token");

            // Config for URL generation
            _config.Setup(x => x["UiUrl"]).Returns("http://localhost:4200");

            // Mapper
            _mapper.Setup(m => m.Map<UserDto>(user))
                   .Returns(new UserDto { Email = dto.Email });

            // Act
            var result = await _controller.ForgetPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("reset-token", response.Token);
        }

        [Fact]
        [Trait("AccountController", "ResendResetPassword")]
        public async Task ResendResetPassword_UserNotFound_Returns500()
        {
            // Arrange
            var email = "test@test.com";
            _userManager.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.ResendResetPassword(email);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        [Trait("AccountController", "ResendResetPassword")]
        public async Task ResendResetPassword_Success_ReturnsOk()
        {
            // Arrange
            var email = "test@test.com";
            var user = new ApplicationUser { Email = email };

            _userManager.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                        .ReturnsAsync("reset-token");

            _config.Setup(x => x["UiUrl"]).Returns("http://localhost:4200");

            _mapper.Setup(m => m.Map<UserDto>(user))
                   .Returns(new UserDto { Email = email });

            // Act
            var result = await _controller.ResendResetPassword(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("reset-token", response.Token);
        }

        [Fact]
        [Trait("AccountController", "ResetPassword")]
        public async Task ResetPassword_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Token = "token", NewPassword = "new" };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "ResetPassword")]
        public async Task ResetPassword_ResetFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Token = "token", NewPassword = "new" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "ResetPassword")]
        public async Task ResetPassword_Success_ReturnsOk()
        {
            // Arrange
            var dto = new ResetPasswordDto { Email = "test@test.com", Token = "token", NewPassword = "new" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Reset password done successfully!", okResult.Value);
        }
    }
}
