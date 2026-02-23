namespace Ecommerce.API.Dtos.Requests
{
    public class PermissionCheckboxDto
    {
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}