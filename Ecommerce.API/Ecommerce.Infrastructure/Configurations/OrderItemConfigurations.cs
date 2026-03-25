
namespace Ecommerce.Infrastructure.Configurations
{
    public class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.OwnsOne(oi => oi.ProductItemOrdered, oi =>
            {
                oi.WithOwner();
                oi.Property(p => p.DiscountPercentage)
                    .HasColumnType("decimal(18,2)");
            });

            builder.Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
