using AutoMapper;
using Ecommerce.API.Controllers;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using Xunit;
using Ecommerce.UnitTests.Helpers;

namespace Ecommerce.UnitTests.ControllerTests
{
    public class RolesControllerTests
    {
        private readonly Mock<RoleManager<IdentityRole>> _roleManager;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IPermissionService> _permissionService;
        private readonly RolesController _controller;

        public RolesControllerTests()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _mapper = new Mock<IMapper>();
            _permissionService = new Mock<IPermissionService>();

            _controller = new RolesController(
                _roleManager.Object,
                _userManager.Object,
                _mapper.Object,
                _permissionService.Object);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkWithRoles()
        {
            // Arrange
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "1", Name = "Admin" },
                new IdentityRole { Id = "2", Name = "Customer" }
            };
            var roleDtos = new List<RoleDto>
            {
                new RoleDto { Id = "1", Name = "Admin", UserCount = 0 },
                new RoleDto { Id = "2", Name = "Customer", UserCount = 0 }
            };

            _roleManager.Setup(r => r.Roles).Returns(new TestAsyncEnumerable<IdentityRole>(roles));
            _mapper.Setup(m => m.Map<List<RoleDto>>(It.IsAny<List<IdentityRole>>())).Returns(roleDtos);
            _userManager.Setup(u => u.GetUsersInRoleAsync(It.IsAny<string>()))
                       .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _controller.GetAllRoles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnRoles = Assert.IsAssignableFrom<ICollection<RoleDto>>(okResult.Value);
            Assert.Equal(2, returnRoles.Count);
        }

        [Fact]
        public async Task GetRoleById_ExistingRole_ReturnsOkWithRole()
        {
            // Arrange
            var roleId = "1";
            var role = new IdentityRole { Id = roleId, Name = "Admin" };
            var roleDto = new RoleDto { Id = roleId, Name = "Admin", UserCount = 0 };

            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
            _mapper.Setup(m => m.Map<RoleDto>(role)).Returns(roleDto);
            _userManager.Setup(u => u.GetUsersInRoleAsync(role.Name!))
                       .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _controller.GetRoleById(roleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnRole = Assert.IsType<RoleDto>(okResult.Value);
            Assert.Equal(roleId, returnRole.Id);
        }

        [Fact]
        public async Task GetRoleById_NonExistingRole_ReturnsNotFound()
        {
            // Arrange
            var roleId = "1";
            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync((IdentityRole?)null);

            // Act
            var result = await _controller.GetRoleById(roleId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
            Assert.Contains("Role not found", apiResponse.Message);
        }

        [Fact]
        public async Task Create_ValidRole_ReturnsCreated()
        {
            // Arrange
            var dto = new RoleToCreateDto { Name = "NewRole" };
            var role = new IdentityRole { Id = "1", Name = "NewRole" };
            var roleDto = new RoleDto { Id = "1", Name = "NewRole" };

            _roleManager.Setup(r => r.RoleExistsAsync(dto.Name)).ReturnsAsync(false);
            _mapper.Setup(m => m.Map<IdentityRole>(dto)).Returns(role);
            _roleManager.Setup(r => r.CreateAsync(role)).ReturnsAsync(IdentityResult.Success);
            _mapper.Setup(m => m.Map<RoleDto>(role)).Returns(roleDto);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(RolesController.GetRoleById), createdResult.ActionName);
            Assert.Equal(role.Id, createdResult.RouteValues!["id"]);
            var returnRole = Assert.IsType<RoleDto>(createdResult.Value);
            Assert.Equal("NewRole", returnRole.Name);
            _roleManager.Verify(r => r.CreateAsync(role), Times.Once);
        }

        [Fact]
        public async Task Create_DuplicateRole_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RoleToCreateDto { Name = "Admin" };
            _roleManager.Setup(r => r.RoleExistsAsync(dto.Name)).ReturnsAsync(true);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("Role already exists", apiResponse.Message);
        }

        [Fact]
        public async Task Delete_RoleWithNoUsers_ReturnsNoContent()
        {
            // Arrange
            var roleId = "1";
            var role = new IdentityRole { Id = roleId, Name = "TestRole" };

            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
            _userManager.Setup(u => u.GetUsersInRoleAsync(role.Name!))
                       .ReturnsAsync(new List<ApplicationUser>());
            _roleManager.Setup(r => r.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Delete(roleId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _roleManager.Verify(r => r.DeleteAsync(role), Times.Once);
        }

        [Fact]
        public async Task Delete_RoleWithUsers_ReturnsBadRequest()
        {
            // Arrange
            var roleId = "1";
            var role = new IdentityRole { Id = roleId, Name = "Admin" };
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", Email = "user1@example.com" }
            };

            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
            _userManager.Setup(u => u.GetUsersInRoleAsync(role.Name!)).ReturnsAsync(users);

            // Act
            var result = await _controller.Delete(roleId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Contains("Cannot delete role with assigned users", apiResponse.Message);
        }

        [Fact]
        public async Task Delete_NonExistingRole_ReturnsNotFound()
        {
            // Arrange
            var roleId = "1";
            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync((IdentityRole?)null);

            // Act
            var result = await _controller.Delete(roleId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
            Assert.Contains("Role not found", apiResponse.Message);
        }

        [Fact]
        public async Task GetManageUserRoles_ValidUser_ReturnsOkWithUserRoles()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId, Email = "test@example.com" };
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "1", Name = "Admin" },
                new IdentityRole { Id = "2", Name = "Customer" }
            };
            var userRoles = new List<string> { "Admin" };
            var checkBoxRoles = new List<CheckBoxRoleManageDto>
            {
                new CheckBoxRoleManageDto { RoleId = "1", RoleName = "Admin", IsSelected = true },
                new CheckBoxRoleManageDto { RoleId = "2", RoleName = "Customer", IsSelected = false }
            };
            var userRolesDto = new UserRolesDto { UserId = userId, Roles = checkBoxRoles };

            _userManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _roleManager.Setup(r => r.Roles).Returns(new TestAsyncEnumerable<IdentityRole>(roles));
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(userRoles);
            _mapper.Setup(m => m.Map<List<CheckBoxRoleManageDto>>(roles)).Returns(checkBoxRoles);
            _mapper.Setup(m => m.Map<UserRolesDto>(user)).Returns(userRolesDto);

            // Act
            var result = await _controller.GetManageUserRoles(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUserRoles = Assert.IsType<UserRolesDto>(okResult.Value);
            Assert.Equal(userId, returnUserRoles.UserId);
            Assert.Equal(2, returnUserRoles.Roles.Count);
        }

        [Fact]
        public async Task GetManageUserRoles_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = "nonexistent";
            _userManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.GetManageUserRoles(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateRoles_ValidDto_ReturnsOkWithUpdatedRoles()
        {
            // Arrange
            var userRolesDto = new UserRolesDto
            {
                UserId = "user1",
                Roles = new List<CheckBoxRoleManageDto>
                {
                    new CheckBoxRoleManageDto { RoleId = "1", RoleName = "Admin", IsSelected = true },
                    new CheckBoxRoleManageDto { RoleId = "2", RoleName = "Customer", IsSelected = false }
                }
            };
            var user = new ApplicationUser { Id = "user1", Email = "test@example.com" };
            var currentRoles = new List<string> { "Customer" };

            _userManager.Setup(u => u.FindByIdAsync(userRolesDto.UserId)).ReturnsAsync(user);
            _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(currentRoles);
            _userManager.Setup(u => u.RemoveFromRolesAsync(user, currentRoles))
                       .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(u => u.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                       .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UpdateRoles(userRolesDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUserRoles = Assert.IsType<UserRolesDto>(okResult.Value);
            Assert.Equal(userRolesDto.UserId, returnUserRoles.UserId);
            _userManager.Verify(u => u.RemoveFromRolesAsync(user, currentRoles), Times.Once);
            _userManager.Verify(u => u.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetManagePermissions_ValidRole_ReturnsOkWithPermissions()
        {
            // Arrange
            var roleId = "1";
            var role = new IdentityRole { Id = roleId, Name = "Admin" };
            var rolePermissions = new HashSet<string> { "Permissions.Products.Read" };
            var rolePermissionsDto = new RolePermissionsDto
            {
                RoleId = roleId,
                RoleName = "Admin",
                Permissions = new List<PermissionCheckboxDto>()
            };

            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
            _permissionService.Setup(p => p.GetRolePermissionsAsync(role)).ReturnsAsync(rolePermissions);
            _mapper.Setup(m => m.Map<RolePermissionsDto>(role)).Returns(rolePermissionsDto);

            // Act
            var result = await _controller.GetManagePermissions(roleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnPermissions = Assert.IsType<RolePermissionsDto>(okResult.Value);
            Assert.Equal(roleId, returnPermissions.RoleId);
        }

        [Fact]
        public async Task UpdatePermissions_ValidDto_ReturnsOkWithUpdatedPermissions()
        {
            // Arrange
            var roleId = "1";
            var role = new IdentityRole { Id = roleId, Name = "Admin" };
            var permissions = new List<PermissionCheckboxDto>
            {
                new PermissionCheckboxDto { PermissionName = "Permissions.Products.Read", IsSelected = true },
                new PermissionCheckboxDto { PermissionName = "Permissions.Products.Create", IsSelected = false }
            };
            var rolePermissionsDto = new RolePermissionsDto { RoleId = roleId, RoleName = "Admin", Permissions = permissions };

            _roleManager.Setup(r => r.FindByIdAsync(roleId)).ReturnsAsync(role);
            _mapper.Setup(m => m.Map<RolePermissionsDto>(role)).Returns(rolePermissionsDto);

            // Act
            var result = await _controller.UpdatePermissions(roleId, permissions);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnPermissions = Assert.IsType<RolePermissionsDto>(okResult.Value);
            Assert.Equal(roleId, returnPermissions.RoleId);
            _permissionService.Verify(p => p.RemoveAllPermissionsAsync(role), Times.Once);
            _permissionService.Verify(p => p.AddPermissionsAsync(role, It.IsAny<IEnumerable<string>>()), Times.Once);
        }
    }


}
