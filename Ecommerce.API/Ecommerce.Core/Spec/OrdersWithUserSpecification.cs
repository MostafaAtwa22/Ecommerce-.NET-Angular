using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Params;

namespace Ecommerce.Core.Spec
{
    public class OrdersWithUserSpecification : BaseSepcifications<Order>
    {
        public OrdersWithUserSpecification(OrdersSpecParams specParams, bool forCount = false)
        {
            if (!forCount)
            {
                Includes.Add(o => o.ApplicationUser);
                switch(specParams.Sort)
                {
                    case "DateAsc": AddOrderBy(o => o.OrderDate); break;
                    case "DateDesc": AddOrderByDesc(o => o.OrderDate); break;
                    default: AddOrderByDesc(o => o.OrderDate); break;
                }
                ApplyPaging((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }
        }
    }
}
