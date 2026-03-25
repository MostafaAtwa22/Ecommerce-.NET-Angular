using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class AdminCreateDto : RegisterDto
    {
        public bool EmailConfirmed { get; set; } = true;
    }
}
