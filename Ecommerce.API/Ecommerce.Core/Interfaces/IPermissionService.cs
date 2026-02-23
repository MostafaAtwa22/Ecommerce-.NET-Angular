
namespace Ecommerce.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetRolePermissionsAsync(IdentityRole role);
        Task RemoveAllPermissionsAsync(IdentityRole role);
        Task AddPermissionsAsync(IdentityRole role, IEnumerable<string> permissions);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<List<string>> GetUserPermissionsAsync(ApplicationUser user);
        Task<bool> HasPermissionAsync(string userId, string permission);
    }
}
