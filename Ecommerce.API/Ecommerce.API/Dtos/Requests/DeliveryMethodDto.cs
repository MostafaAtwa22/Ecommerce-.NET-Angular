using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Dtos.Requests
{
    public class DeliveryMethodDto
    {
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DeliveryTime { get; set; }
        public decimal Price { get; set; }
    }
}