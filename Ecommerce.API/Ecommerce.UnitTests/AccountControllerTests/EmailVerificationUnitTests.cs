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
    public class EmailVerificationUnitTests : AccountControllerTestsBase
    {
        public EmailVerificationUnitTests() : base()
        {
        }

        [Fact]
        [Trait("AccountController", "EmailVerification")]
        public async Task EmailVerification_NullEmailOrCode_ReturnsBadRequest()
        {
            // Arrange
            var dto = new EmailVerficationDto { Email = null!, Code = null! };

            // Act
            var result = await _controller.EmailVerfication(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "EmailVerification")]
        public async Task EmailVerification_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var dto = new EmailVerficationDto { Email = "test@test.com", Code = "code" };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.EmailVerfication(dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "EmailVerification")]
        public async Task EmailVerification_VerificationFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = new EmailVerficationDto { Email = "test@test.com", Code = "code" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.ConfirmEmailAsync(user, dto.Code))
                        .ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.EmailVerfication(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "EmailVerification")]
        public async Task EmailVerification_Success_ReturnsOk()
        {
            // Arrange
            var dto = new EmailVerficationDto { Email = "test@test.com", Code = "code" };
            var user = new ApplicationUser { Email = dto.Email };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.ConfirmEmailAsync(user, dto.Code))
                        .ReturnsAsync(IdentityResult.Success);

            // Mocks for CreateUserResponseAsync
            _mapper.Setup(m => m.Map<UserDto>(user))
                   .Returns(new UserDto { Email = dto.Email });
            
            _tokenService.Setup(t => t.CreateToken(user))
                         .ReturnsAsync("jwt-token");

            _tokenService.Setup(t => t.GenerateRefreshToken())
                         .Returns(new RefreshToken { Token = "refresh", ExpiresOn = DateTime.UtcNow.AddDays(7) });

            _userManager.Setup(x => x.UpdateAsync(user))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.EmailVerfication(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<UserDto>(okResult.Value);
        }

        [Fact]
        [Trait("AccountController", "ResendVerificationEmail")]
        public async Task ResendVerificationEmail_EmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "" };

            // Act
            var result = await _controller.ResendVerificationEmail(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); 
        }

        [Fact]
        [Trait("AccountController", "ResendVerificationEmail")]
        public async Task ResendVerificationEmail_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "test@test.com" };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.ResendVerificationEmail(dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        [Trait("AccountController", "ResendVerificationEmail")]
        public async Task ResendVerificationEmail_AlreadyVerified_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "test@test.com" };
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = true };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            // Act
            var result = await _controller.ResendVerificationEmail(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        [Trait("AccountController", "ResendVerificationEmail")]
        public async Task ResendVerificationEmail_Success_ReturnsOk()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "test@test.com" };
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = false };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                        .ReturnsAsync("token");

            // Act
            var result = await _controller.ResendVerificationEmail(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
