namespace Ecommerce.API.Dtos.Requests
{
    public class RolePermissionsDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionCheckboxDto> Permissions { get; set; } = new();
    }
}