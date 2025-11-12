using Ecommerce.Core.Entities.orderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.OwnsOne(oi => oi.ProductItemOrdered, oi =>
            {
                oi.WithOwner();
            });

            builder.Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
