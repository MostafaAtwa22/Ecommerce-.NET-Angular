using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class RoleToCreateDto
    {
        [Required, MinLength(3), StringLength(50)]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "Role can only contain letters.")]
        public string Name { get; set; } = string.Empty;
    }
}