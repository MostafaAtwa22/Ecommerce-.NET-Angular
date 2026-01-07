using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetRolePermissionsAsync(IdentityRole role);
        Task RemoveAllPermissionsAsync(IdentityRole role);
        Task AddPermissionsAsync(IdentityRole role, IEnumerable<string> permissions);
    }
}