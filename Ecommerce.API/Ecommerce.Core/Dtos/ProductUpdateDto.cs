using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.Dtos
{
    public class ProductUpdateDto : ProductCommonDto
    {
        public int ProductId { get; set; }
        public string? ProductImage { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
