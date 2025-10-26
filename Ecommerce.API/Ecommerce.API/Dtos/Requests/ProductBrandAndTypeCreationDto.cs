using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductBrandAndTypeCreationDto
    {
        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Only letters, numbers, spaces, hyphens and underscores are allowed")]
        public string Name { get; set; } = string.Empty;
    }
}