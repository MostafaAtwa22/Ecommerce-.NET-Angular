
namespace Ecommerce.API.Dtos.Responses
{
    public class LoginResponseDto
    {
        public bool RequiresTwoFactor { get; set; }
        public string? Message { get; set; }
        public string? Email { get; set; }
        public UserDto? User { get; set; }
    }
}
