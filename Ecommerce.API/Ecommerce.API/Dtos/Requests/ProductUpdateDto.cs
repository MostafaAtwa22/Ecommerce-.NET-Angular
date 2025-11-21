using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductUpdateDto : ProductCommonDto
    {
        public int ProductId { get; set; }
        public string? PictureUrl { get; set; } = string.Empty;
    }
}