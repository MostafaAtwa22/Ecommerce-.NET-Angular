namespace Ecommerce.API.Dtos.Responses
{
    public class UserDto : UserCommonDto
    {
        public string Token { get; set; } = string.Empty;
    }
}