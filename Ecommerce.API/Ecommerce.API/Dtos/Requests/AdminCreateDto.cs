using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class AdminCreateDto : RegisterDto
    {
        [Required]
        public string RoleName { get; set; } = Role.Customer.ToString();

        public bool EmailConfirmed { get; set; } = true;
    }
}
