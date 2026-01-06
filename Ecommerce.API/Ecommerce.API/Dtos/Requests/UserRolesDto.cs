namespace Ecommerce.API.Dtos.Requests
{
    public class UserRolesDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<CheckBoxRoleManageDto> Roles { get; set; } = new List<CheckBoxRoleManageDto>();
    }
}