using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductUpdateDto : ProductCommonDto
    {
        public int ProductId { get; set; }
        
        public string? ProductImage { get; set; }

        [AllowedExtensions(FileSettings.AllowedExtensions),
            MaxFileSize(FileSettings.MaxFileSizeInBytes)]
        public IFormFile? ImageFile { get; set; }
    }
}
