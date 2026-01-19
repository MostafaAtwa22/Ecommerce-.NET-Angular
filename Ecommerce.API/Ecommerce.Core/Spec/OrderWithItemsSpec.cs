using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Entities.orderAggregate;

namespace Ecommerce.Core.Spec
{
    public class OrderWithItemsSpec : BaseSepcifications<Order>
    {
        public OrderWithItemsSpec(int orderId)
            : base(o => o.Id == orderId)
        {
            Includes.Add(o => o.OrderItems);
        }
    }
}