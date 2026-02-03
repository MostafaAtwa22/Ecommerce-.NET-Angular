using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System.Security.Claims;

namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class TokenUnitTests : AccountControllerTestsBase
    {
        public TokenUnitTests() : base()
        {
        }

        private void SetupRequestCookies(string key, string value)
        {
            var cookieCollection = new Mock<IRequestCookieCollection>();
            cookieCollection.Setup(c => c[key]).Returns(value);
            
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Cookies).Returns(cookieCollection.Object);
            
            var context = new Mock<HttpContext>();
            context.Setup(c => c.Request).Returns(request.Object);
            
            // Should also setup Response.Cookies for setting cookies
            var response = new Mock<HttpResponse>();
            var responseCookies = new Mock<IResponseCookies>();
            response.Setup(r => r.Cookies).Returns(responseCookies.Object);
            context.Setup(c => c.Response).Returns(response.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context.Object
            };
        }

        [Fact]
        [Trait("AccountController", "RefreshToken")]
        public async Task RefreshToken_NullToken_ReturnsUnauthorized()
        {
            // Arrange
            SetupRequestCookies("ecommerce_refreshToken", "");

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "RevokeToken")]
        public async Task RevokeToken_NullToken_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RevokeTokenDto { Token = null };
            SetupRequestCookies("ecommerce_refreshToken", "");

            // Act
            var result = await _controller.RevokeToken(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        [Trait("AccountController", "Logout")]
        public async Task Logout_MissingToken_ReturnsNoContent()
        {
            // Arrange
            SetupRequestCookies("ecommerce_refreshToken", "");

            // Act
            var result = await _controller.Logout();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
