namespace Ecommerce.API.Dtos.Requests
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; } = 0;
    }
}