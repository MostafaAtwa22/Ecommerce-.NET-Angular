using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Ecommerce.UnitTests.AccountControllerTests
{
    [Collection("Hangfire")]
    public class RegisterUnitTests : AccountControllerTestsBase
    {
        public RegisterUnitTests() : base()
        {
        }

        private RegisterDto ValidDto() => new RegisterDto
        {
            Email = "test@test.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            Gender = Gender.Male,
            PhoneNumber = "1234567890",
            Password = "Test@123",
            ConfirmPassword = "Test@123",
            RoleName = "Customer"
        };

        private void SetupUnauthenticatedUser()
        {
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private void SetupAuthenticatedSuperAdmin()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "superadmin@test.com"),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_ValidCustomer_ReturnsOk()
        {
            // Arrange
            var dto = ValidDto();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            _mapper.Setup(m => m.Map<ApplicationUser>(dto))
                    .Returns(new ApplicationUser { Email = dto.Email, UserName = dto.UserName });

            _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                        .ReturnsAsync(IdentityResult.Success);

            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                        .ReturnsAsync(IdentityResult.Success);

            _userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync("confirmation-token");

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_EmailAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidDto();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync(new ApplicationUser());

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_InvalidRoleName_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidDto();
            dto.RoleName = "InvalidRole";

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_CreateUserFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidDto();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            _mapper.Setup(m => m.Map<ApplicationUser>(dto))
                   .Returns(new ApplicationUser());

            _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Password too weak" }));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_AddToRoleFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = ValidDto();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            _mapper.Setup(m => m.Map<ApplicationUser>(dto))
                   .Returns(new ApplicationUser());

            _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Role assignment failed" }));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_AdminRole_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            var dto = ValidDto();
            dto.RoleName = "Admin";

            SetupUnauthenticatedUser();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_SuperAdminRole_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange
            var dto = ValidDto();
            dto.RoleName = "SuperAdmin";

            SetupUnauthenticatedUser();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("AccountController", "Register")]
        public async Task Register_AdminRole_AuthenticatedSuperAdmin_ReturnsOk()
        {
            // Arrange
            var dto = ValidDto();
            dto.RoleName = "Admin";

            SetupAuthenticatedSuperAdmin();

            _userManager.Setup(x => x.FindByEmailAsync(dto.Email))
                        .ReturnsAsync((ApplicationUser?)null);

            _mapper.Setup(m => m.Map<ApplicationUser>(dto))
                   .Returns(new ApplicationUser { Email = dto.Email, UserName = dto.UserName });

            _userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                        .ReturnsAsync(IdentityResult.Success);

            _userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
                        .ReturnsAsync(IdentityResult.Success);

            _userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                        .ReturnsAsync("confirmation-token");

            // Act
            var result = await _controller.Register(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
