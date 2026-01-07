using System.Security.Claims;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infrastructure.Services
{
public class PermissionService : IPermissionService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
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
    }
}