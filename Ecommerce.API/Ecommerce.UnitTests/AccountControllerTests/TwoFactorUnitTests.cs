
namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class TwoFactorUnitTests : AccountControllerTestsBase
    {
        public TwoFactorUnitTests() : base()
        {
        }

        [Fact]
        [Trait("AccountController", "Verify2FA")]
        public async Task Verify2FA_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new Verify2FADto { Email = "test@test.com", Code = "123456" };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Verify2FA(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Verify2FA")]
        public async Task Verify2FA_2FANotEnabled_ReturnsBadRequest()
        {
            // Arrange
            var dto = new Verify2FADto { Email = "test@test.com", Code = "123456" };
            var user = new ApplicationUser { Email = dto.Email, TwoFactorEnabled = false };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            // Act
            var result = await _controller.Verify2FA(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Verify2FA")]
        public async Task Verify2FA_InvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var dto = new Verify2FADto { Email = "test@test.com", Code = "123456" };
            var user = new ApplicationUser { Email = dto.Email, TwoFactorEnabled = true };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);
            
            _userManager.Setup(x => x.VerifyTwoFactorTokenAsync(user, "Email", dto.Code))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.Verify2FA(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Verify2FA")]
        public async Task Verify2FA_ValidCode_ReturnsOk()
        {
            // Arrange
            var dto = new Verify2FADto { Email = "test@test.com", Code = "123456" };
            var user = new ApplicationUser { Email = dto.Email, TwoFactorEnabled = true };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);
            
            _userManager.Setup(x => x.VerifyTwoFactorTokenAsync(user, "Email", dto.Code))
                        .ReturnsAsync(true);

            _mapper.Setup(m => m.Map<UserDto>(user))
                   .Returns(new UserDto { Email = dto.Email });

            _tokenService.Setup(t => t.CreateToken(user))
                         .ReturnsAsync("test-jwt-token");

            _tokenService.Setup(t => t.GenerateRefreshToken())
                         .Returns(new RefreshToken { Token = "refresh-token", ExpiresOn = DateTime.UtcNow.AddDays(7) });
            
            _userManager.Setup(x => x.UpdateAsync(user))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Verify2FA(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<UserDto>(okResult.Value);
        }

        [Fact]
        [Trait("AccountController", "Resend2FA")]
        public async Task Resend2FA_MissingEmail_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "" };

            // Act
            var result = await _controller.Resend2FA(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Resend2FA")]
        public async Task Resend2FA_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "test@test.com" };
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Resend2FA(dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Resend2FA")]
        public async Task Resend2FA_2FANotEnabled_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "test@test.com" };
            var user = new ApplicationUser { Email = dto.Email, TwoFactorEnabled = false };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            // Act
            var result = await _controller.Resend2FA(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Resend2FA")]
        public async Task Resend2FA_Valid_ReturnsOk()
        {
            // Arrange
            var dto = new ResendVerificationEmailDto { Email = "test@test.com" };
            var user = new ApplicationUser { Email = dto.Email, TwoFactorEnabled = true };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.GenerateTwoFactorTokenAsync(user, "Email"))
                        .ReturnsAsync("123456");

            // Act
            var result = await _controller.Resend2FA(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
