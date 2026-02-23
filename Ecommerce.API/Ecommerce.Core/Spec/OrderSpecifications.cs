
namespace Ecommerce.Core.Spec
{
    public static class OrderSpecifications
    {
        public static ISpecifications<Order> BuildListingSpec(OrdersSpecParams specParams)
        {
            var builder = new SpecificationBuilder<Order>()
                .Include(o => o.ApplicationUser);

            switch (specParams.Sort)
            {
                case "DateAsc":
                    builder.OrderByAsc(o => o.OrderDate);
                    break;
                case "DateDesc":
                default:
                    builder.OrderByDesc(o => o.OrderDate);
                    break;
            }

            builder.Paginate(specParams.PageIndex, specParams.PageSize);

            return builder;
        }

        public static ISpecifications<Order> BuildListingCountSpec(OrdersSpecParams specParams)
        {
            var builder = new SpecificationBuilder<Order>();
            return builder;
        }

        public static ISpecifications<Order> BuildDetailsSpec(int orderId)
        {
            return new SpecificationBuilder<Order>(o => o.Id == orderId)
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryMethod);
        }

        public static ISpecifications<Order> BuildOrderWithItemsSpec(int orderId)
        {
            return new SpecificationBuilder<Order>(o => o.Id == orderId)
                .Include(o => o.OrderItems);
        }
    }
}

