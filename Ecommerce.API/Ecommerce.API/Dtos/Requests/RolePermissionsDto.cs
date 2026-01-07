namespace Ecommerce.API.Dtos.Requests
{
    public class RolePermissionsDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionCheckboxDto> Permissions { get; set; } = new();
    }
}