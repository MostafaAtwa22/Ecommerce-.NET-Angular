using System.Security.Claims;
using Ecommerce.API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class PermissionsUnitTests : AccountControllerTestsBase
    {
        [Fact]
        public async Task GetUserPermissions_AuthenticatedUser_ReturnsPermissions()
        {
            // Arrange
            var userId = "test-user-id";
            var expectedPermissions = new List<string>
            {
                "Permissions.Products.Read",
                "Permissions.Products.Create",
                "Permissions.Orders.Read"
            };

            // Mock user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            _permissionService
                .Setup(x => x.GetUserPermissionsAsync(userId))
                .ReturnsAsync(expectedPermissions);

            // Act
            var result = await _controller.GetUserPermissions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var permissions = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(expectedPermissions.Count, permissions.Count);
            Assert.Equal(expectedPermissions, permissions);
        }

        [Fact]
        public async Task GetUserPermissions_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // No claims
                }
            };

            // Act
            var result = await _controller.GetUserPermissions();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(unauthorizedResult.Value);
            Assert.Equal(StatusCodes.Status401Unauthorized, apiResponse.StatusCode);
        }

        [Fact]
        public async Task GetUserPermissions_UserWithNoPermissions_ReturnsEmptyList()
        {
            // Arrange
            var userId = "test-user-id";
            var expectedPermissions = new List<string>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            _permissionService
                .Setup(x => x.GetUserPermissionsAsync(userId))
                .ReturnsAsync(expectedPermissions);

            // Act
            var result = await _controller.GetUserPermissions();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var permissions = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(permissions);
        }

        [Fact]
        public async Task GetUserPermissions_CallsPermissionServiceCorrectly()
        {
            // Arrange
            var userId = "test-user-id";
            var permissions = new List<string> { "Permissions.Products.Read" };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            _permissionService
                .Setup(x => x.GetUserPermissionsAsync(userId))
                .ReturnsAsync(permissions);

            // Act
            await _controller.GetUserPermissions();

            // Assert
            _permissionService.Verify(
                x => x.GetUserPermissionsAsync(userId),
                Times.Once);
        }
    }
}
