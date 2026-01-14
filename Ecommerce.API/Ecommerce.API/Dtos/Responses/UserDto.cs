using Newtonsoft.Json;

namespace Ecommerce.API.Dtos.Responses
{
    public class UserDto : UserCommonDto
    {
        public string Token { get; set; } = string.Empty;

        [JsonIgnore]
        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }
    }
}