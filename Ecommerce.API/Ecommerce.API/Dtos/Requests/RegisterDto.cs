using System.ComponentModel.DataAnnotations;
using Ecommerce.Core.Constants;
using Ecommerce.Core.Enums;

namespace Ecommerce.API.Dtos.Requests
{
    public class RegisterDto
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", 
            ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression(@"^[A-Za-z]+$", 
            ErrorMessage = "First name can only contain letters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "Last name can only contain letters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        [Phone]
        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$",
            ErrorMessage = "Password must be at least 6 characters long and include an uppercase, lowercase, number, and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = Role.Customer.ToString();
    }
}
