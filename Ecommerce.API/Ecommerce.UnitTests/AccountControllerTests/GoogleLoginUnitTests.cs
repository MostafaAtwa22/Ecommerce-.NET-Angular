using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.googleDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class GoogleLoginUnitTests : AccountControllerTestsBase
    {
        public GoogleLoginUnitTests() : base()
        {
        }

        [Fact]
        [Trait("AccountController", "GoogleLogin")]
        public async Task GoogleLogin_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new GoogleLoginDto { IdToken = "invalid-token" };
            _googleService.Setup(x => x.ValidateGoogleToken(dto.IdToken))
                          .ReturnsAsync((Payload?)null);

            // Act
            var result = await _controller.GoogleLogin(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "GoogleLogin")]
        public async Task GoogleLogin_UserCreationFailed_ReturnsBadRequest()
        {
            // Arrange
            var dto = new GoogleLoginDto { IdToken = "valid-token" };
            var payload = new Payload { Email = "test@test.com", EmailVerified = true };

            _googleService.Setup(x => x.ValidateGoogleToken(dto.IdToken))
                          .ReturnsAsync(payload);

            _tokenService.Setup(x => x.FindOrCreateUserByGoogleIdAsync(It.IsAny<GoogleUserDto>()))
                         .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.GoogleLogin(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "GoogleLogin")]
        public async Task GoogleLogin_Success_ReturnsOk()
        {
            // Arrange
            var dto = new GoogleLoginDto { IdToken = "valid-token" };
            var payload = new Payload { Email = "test@test.com", EmailVerified = true, Picture = "url" };
            var appUser = new ApplicationUser { Email = "test@test.com" };

            _googleService.Setup(x => x.ValidateGoogleToken(dto.IdToken))
                          .ReturnsAsync(payload);

            _tokenService.Setup(x => x.FindOrCreateUserByGoogleIdAsync(It.IsAny<GoogleUserDto>()))
                         .ReturnsAsync(appUser);

            _mapper.Setup(m => m.Map<UserDto>(appUser))
                   .Returns(new UserDto { Email = appUser.Email });

            _tokenService.Setup(t => t.CreateToken(appUser))
                         .ReturnsAsync("jwt-token");

            _tokenService.Setup(t => t.GenerateRefreshToken())
                         .Returns(new RefreshToken { Token = "refresh", ExpiresOn = DateTime.UtcNow.AddDays(7) });

            _userManager.Setup(x => x.UpdateAsync(appUser))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.GoogleLogin(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(payload.Picture, response.ProfilePicture);
        }
    }
}
