using System.Linq.Expressions;
using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.Core.Spec
{
    public class OrderWithOrderItemsAndDeliverySpec : BaseSepcifications<Order>
    {
        public OrderWithOrderItemsAndDeliverySpec(string email)
            : base (o => o.BuyerEmail == email)
        {
            Include();
            AddOrderByDesc(o => o.OrderDate);
        }

        public OrderWithOrderItemsAndDeliverySpec(string email, int id) 
            : base(o => o.Id == id && o.BuyerEmail == email)
        {
            Include();
        }

        private void Include()
        {
            AddIncludes(o => o.DeliveryMethod);
            AddIncludes(o => o.OrderItems);
        }
    }
}