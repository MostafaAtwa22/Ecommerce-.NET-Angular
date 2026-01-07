namespace Ecommerce.API.Dtos.Requests
{
    public class CheckBoxRoleManageDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}