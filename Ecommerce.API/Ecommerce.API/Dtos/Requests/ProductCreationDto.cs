using System.ComponentModel.DataAnnotations;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Infrastructure.Settings;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductCreationDto : ProductCommonDto
    {
        [Required]
        [AllowedExtensions(FileSettings.AllowedExtensions),
            MaxFileSize(FileSettings.MaxFileSizeInBytes)]
        public IFormFile ImageFile { get; set; } = default!;
    }
}