using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.API.Dtos.Requests
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}