
namespace Ecommerce.UnitTests.ServiceTests
{
    public class PermissionServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<RoleManager<IdentityRole>> _roleManager;
        private readonly Mock<IRedisRepository<List<string>>> _redisRepository;
        private readonly PermissionService _permissionService;

        public PermissionServiceTests()
        {
            _userManager = MockUserManager();
            _roleManager = MockRoleManager();
            _redisRepository = new Mock<IRedisRepository<List<string>>>();
            _permissionService = new PermissionService(
                _roleManager.Object, 
                _userManager.Object,
                _redisRepository.Object
            );
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ByUserId_ReturnsCorrectPermissions()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            };

            var roleName = "Admin";
            var role = new IdentityRole(roleName);
            var permissions = new List<Claim>
            {
                new Claim(Permissions.ClaimType, "Permissions.Products.Read"),
                new Claim(Permissions.ClaimType, "Permissions.Products.Create")
            };

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { roleName });

            _roleManager
                .Setup(x => x.FindByNameAsync(roleName))
                .ReturnsAsync(role);

            _roleManager
                .Setup(x => x.GetClaimsAsync(role))
                .ReturnsAsync(permissions);

            // Act
            var result = await _permissionService.GetUserPermissionsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Permissions.Products.Read", result);
            Assert.Contains("Permissions.Products.Create", result);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ByUserId_UserNotFound_ReturnsEmpty()
        {
            // Arrange
            var userId = "non-existent-user";

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _permissionService.GetUserPermissionsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ByUser_MultipleRoles_AggregatesPermissions()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };

            var role1 = new IdentityRole("Admin");
            var role2 = new IdentityRole("Editor");

            var permissions1 = new List<Claim>
            {
                new Claim(Permissions.ClaimType, "Permissions.Products.Read"),
                new Claim(Permissions.ClaimType, "Permissions.Products.Create")
            };

            var permissions2 = new List<Claim>
            {
                new Claim(Permissions.ClaimType, "Permissions.Orders.Read"),
                new Claim(Permissions.ClaimType, "Permissions.Products.Read") // Duplicate
            };

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "Editor" });

            _roleManager
                .Setup(x => x.FindByNameAsync("Admin"))
                .ReturnsAsync(role1);

            _roleManager
                .Setup(x => x.FindByNameAsync("Editor"))
                .ReturnsAsync(role2);

            _roleManager
                .Setup(x => x.GetClaimsAsync(role1))
                .ReturnsAsync(permissions1);

            _roleManager
                .Setup(x => x.GetClaimsAsync(role2))
                .ReturnsAsync(permissions2);

            // Act
            var result = await _permissionService.GetUserPermissionsAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); 
            Assert.Contains("Permissions.Products.Read", result);
            Assert.Contains("Permissions.Products.Create", result);
            Assert.Contains("Permissions.Orders.Read", result);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ByUser_NoRoles_ReturnsEmpty()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _permissionService.GetUserPermissionsAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_RoleNotFound_SkipsRole()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "NonExistentRole" });

            _roleManager
                .Setup(x => x.FindByNameAsync("NonExistentRole"))
                .ReturnsAsync((IdentityRole?)null);

            // Act
            var result = await _permissionService.GetUserPermissionsAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task HasPermissionAsync_UserHasPermission_ReturnsTrue()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser { Id = userId };
            var role = new IdentityRole("Admin");
            var permissions = new List<Claim>
            {
                new Claim(Permissions.ClaimType, "Permissions.Products.Read")
            };

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            _roleManager
                .Setup(x => x.FindByNameAsync("Admin"))
                .ReturnsAsync(role);

            _roleManager
                .Setup(x => x.GetClaimsAsync(role))
                .ReturnsAsync(permissions);

            // Act
            var result = await _permissionService.HasPermissionAsync(userId, "Permissions.Products.Read");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasPermissionAsync_UserDoesNotHavePermission_ReturnsFalse()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser { Id = userId };
            var role = new IdentityRole("Admin");
            var permissions = new List<Claim>
            {
                new Claim(Permissions.ClaimType, "Permissions.Products.Read")
            };

            _userManager
                .Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            _roleManager
                .Setup(x => x.FindByNameAsync("Admin"))
                .ReturnsAsync(role);

            _roleManager
                .Setup(x => x.GetClaimsAsync(role))
                .ReturnsAsync(permissions);

            // Act
            var result = await _permissionService.HasPermissionAsync(userId, "Permissions.Products.Delete");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_CacheHit_ReturnsCachedPermissions()
        {
            // Arrange
            var userId = "test-user-id";
            var user = new ApplicationUser { Id = userId };
            var cachedPermissions = new List<string> { "Permissions.Products.Read" };
            var cacheKey = $"permissions:user:{userId}";

            _userManager.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            _redisRepository
                .Setup(x => x.GetAsync(cacheKey))
                .ReturnsAsync(cachedPermissions);

            // Act
            var result = await _permissionService.GetUserPermissionsAsync(userId);

            // Assert
            Assert.Equal(cachedPermissions, result);
            
            // Verify DB was NOT queried
            _userManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object, null!, null!, null!, null!);
        }
    }
}
