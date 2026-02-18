
namespace Ecommerce.Infrastructure.Services
{
public class PermissionService : IPermissionService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRedisRepository<List<string>> _redisRepository;

        public PermissionService(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IRedisRepository<List<string>> redisRepository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _redisRepository = redisRepository;
        }

        public async Task<HashSet<string>> GetRolePermissionsAsync(IdentityRole role)
        {
            var claims = await _roleManager.GetClaimsAsync(role);

            return claims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();
        }

        public async Task RemoveAllPermissionsAsync(IdentityRole role)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = claims.Where(c => c.Type == Permissions.ClaimType);

            foreach (var claim in permissionClaims)
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim);
                if (!result.Succeeded)
                    throw new InvalidOperationException("Failed to remove role permissions");
            }
        }

        public async Task AddPermissionsAsync(IdentityRole role, IEnumerable<string> permissions)
        {
            foreach (var permission in permissions)
            {
                var result = await _roleManager.AddClaimAsync(
                    role,
                    new Claim(Permissions.ClaimType, permission)
                );

                if (!result.Succeeded)
                    throw new InvalidOperationException("Failed to add role permissions");
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return await GetUserPermissionsAsync(user);
        }

        public async Task<List<string>> GetUserPermissionsAsync(ApplicationUser user)
        {
            var cacheKey = $"permissions:user:{user.Id}";
            var cachedPermissions = await _redisRepository.GetAsync(cacheKey);

            if (cachedPermissions != null)
                return cachedPermissions;

            var userRoles = await _userManager.GetRolesAsync(user);
            var permissions = new HashSet<string>();

            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    var rolePermissions = roleClaims
                        .Where(c => c.Type == Permissions.ClaimType)
                        .Select(c => c.Value);

                    foreach (var permission in rolePermissions)
                        permissions.Add(permission);
                }
            }

            var result = permissions.ToList();
            
            // Cache for 5 minutes
            await _redisRepository.UpdateOrCreateAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return userPermissions.Contains(permission);
        }
    }
}
