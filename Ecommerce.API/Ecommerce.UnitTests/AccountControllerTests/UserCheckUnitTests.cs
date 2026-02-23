
namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class UserCheckUnitTests : AccountControllerTestsBase
    {
        public UserCheckUnitTests() : base()
        {
        }

        [Fact]
        [Trait("AccountController", "CheckEmailExistsAsync")]
        public async Task CheckEmailExists_ReturnsTrue_WhenEmailExists()
        {
            // Arrange
            var email = "test@test.com";
            _userManager.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync(new ApplicationUser());

            // Act
            var result = await _controller.CheckEmailExistsAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        [Trait("AccountController", "CheckEmailExistsAsync")]
        public async Task CheckEmailExists_ReturnsFalse_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "test@test.com";
            _userManager.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.CheckEmailExistsAsync(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        [Trait("AccountController", "CheckUsernameExistsAsync")]
        public async Task CheckUsernameExists_ReturnsTrue_WhenUsernameExists()
        {
            // Arrange
            var username = "testuser";
            _userManager.Setup(x => x.FindByNameAsync(username))
                        .ReturnsAsync(new ApplicationUser());

            // Act
            var result = await _controller.CheckUsernameExistsAsync(username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        [Trait("AccountController", "CheckUsernameExistsAsync")]
        public async Task CheckUsernameExists_ReturnsFalse_WhenUsernameDoesNotExist()
        {
            // Arrange
            var username = "testuser";
            _userManager.Setup(x => x.FindByNameAsync(username))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.CheckUsernameExistsAsync(username);

            // Assert
            Assert.False(result);
        }
    }
}
