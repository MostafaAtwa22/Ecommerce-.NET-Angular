using System.ComponentModel.DataAnnotations;
using Ecommerce.Core.Entities;

namespace Ecommerce.API.Dtos.Requests
{
    public class BasketItemDto : CommonItemDto
    {
        [Range(minimum: 1, maximum: 100_000)]
        [Required]
        public int Quantity { get; set; }
    }
}