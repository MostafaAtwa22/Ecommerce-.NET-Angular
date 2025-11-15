using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class OrderDto
    {
        [Required]
        public string BasketId { get; set; } = string.Empty;

        [Required]
        public int DeliveryMethodId { get; set; }

        [Required]
        public OrderAddressDto ShipToAddress { get; set; } = new();
    }
}