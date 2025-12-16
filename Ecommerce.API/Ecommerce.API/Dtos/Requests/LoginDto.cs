using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class LoginDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class GoogleLoginDto 
    {
        public string IdToken { get; set; } = string.Empty;
    }
}