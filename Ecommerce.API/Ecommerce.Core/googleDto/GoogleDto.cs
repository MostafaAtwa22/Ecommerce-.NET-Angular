
namespace Ecommerce.Core.googleDto
{
    public class GoogleUserDto
    {
        [Required]
        public string GoogleId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}
