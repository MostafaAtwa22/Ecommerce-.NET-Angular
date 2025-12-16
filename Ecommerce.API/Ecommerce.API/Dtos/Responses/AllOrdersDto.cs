using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.API.Dtos.Responses
{
    public class AllOrdersDto
    {
        public int Id { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}