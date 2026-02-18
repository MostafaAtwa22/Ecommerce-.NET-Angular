
namespace Ecommerce.API.Helpers.Attributes
{
    public class AuthorizePermissionAttribute : AuthorizeAttribute
    {
        public AuthorizePermissionAttribute(string module, string action)
        {
            Policy = $"Permissions.{module}.{action}";
        }
    }
}
