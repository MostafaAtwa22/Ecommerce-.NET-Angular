namespace Ecommerce.API.Dtos.Requests
{
    public class CheckBoxRoleManageDto
    {
        public Guid RoleId { get; set; } 
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}