
namespace Ecommerce.API.Filters
{
    public class PermissionRequirementFilter : IAuthorizationRequirement
    {
        public string Permission { get; private set; } = string.Empty;

        public PermissionRequirementFilter(string permission)
        {
            Permission = permission;
        }
    }
}
