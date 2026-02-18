
namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class LoginUnitTests : AccountControllerTestsBase
    {
        public LoginUnitTests() : base()
        {
        }

        private LoginDto ValidLoginDto() => new LoginDto
        {
            Email = "test@test.com",
            Password = "Test@123"
        };


        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var dto = ValidLoginDto();
            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_EmailNotConfirmed_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidLoginDto();
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = false };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.IsEmailConfirmedAsync(user))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_UserLockedOut_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidLoginDto();
            var user = new ApplicationUser
            {
                Email = dto.Email,
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.IsEmailConfirmedAsync(user))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidLoginDto();
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = true };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.IsEmailConfirmedAsync(user))
                        .ReturnsAsync(true);

            _signInManager.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, true))
                          .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_LockedOutAfterFailure_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidLoginDto();
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = true };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.IsEmailConfirmedAsync(user))
                        .ReturnsAsync(true);

            _signInManager.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, true))
                          .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_ValidCredentials_Success_ReturnsOk()
        {
            // Arrange
            var dto = ValidLoginDto();
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = true, TwoFactorEnabled = false };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.IsEmailConfirmedAsync(user))
                        .ReturnsAsync(true);

            _signInManager.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, true))
                          .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _mapper.Setup(m => m.Map<UserDto>(user))
                   .Returns(new UserDto { Email = dto.Email });

            _tokenService.Setup(t => t.CreateToken(user))
                         .ReturnsAsync("test-jwt-token");

            _tokenService.Setup(t => t.GenerateRefreshToken())
                         .Returns(new RefreshToken { Token = "refresh-token", ExpiresOn = DateTime.UtcNow.AddDays(7) });
            
            _userManager.Setup(x => x.UpdateAsync(user))
                        .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LoginResponseDto>(okResult.Value);
            
            Assert.False(response.RequiresTwoFactor);
            Assert.NotNull(response.User);
            Assert.Equal(dto.Email, response.User.Email);
        }

        [Fact]
        [Trait("AccountController", "Login")]
        public async Task Login_ValidCredentials_2FARequired_ReturnsOk()
        {
            // Arrange
            var dto = ValidLoginDto();
            var user = new ApplicationUser { Email = dto.Email, EmailConfirmed = true, TwoFactorEnabled = true };

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(user);

            _userManager.Setup(x => x.IsEmailConfirmedAsync(user))
                        .ReturnsAsync(true);

            _signInManager.Setup(x => x.CheckPasswordSignInAsync(user, dto.Password, true))
                          .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _userManager.Setup(x => x.GenerateTwoFactorTokenAsync(user, "Email"))
                        .ReturnsAsync("123456");

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.True(response.RequiresTwoFactor);
            Assert.Null(response.User);
        }
    }
}
