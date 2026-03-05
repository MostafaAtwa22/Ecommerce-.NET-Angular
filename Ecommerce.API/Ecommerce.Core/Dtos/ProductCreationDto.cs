using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.Dtos
{
    public class ProductCreationDto : ProductCommonDto
    {
        [Required]
        public IFormFile ImageFile { get; set; } = default!;
    }
}
