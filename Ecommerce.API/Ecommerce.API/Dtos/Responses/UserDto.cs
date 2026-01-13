using System.Text.Json.Serialization;

namespace Ecommerce.API.Dtos.Responses
{
    public class UserDto : UserCommonDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiration { get; set; }
    }
}