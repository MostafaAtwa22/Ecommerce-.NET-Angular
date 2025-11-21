using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProductCreationDto : ProductCommonDto
    {
        [Required]
        public string PictureUrl { get; set; } = string.Empty;
    }
}