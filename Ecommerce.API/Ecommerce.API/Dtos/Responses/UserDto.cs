namespace Ecommerce.API.Dtos.Responses
{
    public class UserDto : UserCommonDto
    {
        public ICollection<string> Roles { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
    }
}