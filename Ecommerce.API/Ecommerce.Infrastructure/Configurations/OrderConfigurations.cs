using Ecommerce.Core.Entities.orderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(o => o.AddressToShip, o =>
            {
                o.WithOwner();

                o.Property(p => p.FirstName)
                    .HasMaxLength(100);
                o.Property(p => p.LastName)
                    .HasMaxLength(100);
                o.Property(p => p.Country)
                    .HasMaxLength(100);
                o.Property(p => p.Government)
                    .HasMaxLength(100);
                o.Property(p => p.City)
                    .HasMaxLength(100);
                o.Property(p => p.Street)
                    .HasMaxLength(150);
                o.Property(p => p.Zipcode)
                    .HasMaxLength(20);
            });

            builder.Property(o => o.Status)
                .HasConversion(
                    s => s.ToString(),
                    s => (OrderStatus)Enum.Parse(typeof(OrderStatus), s)
                );

            builder.Property(oi => oi.SubTotal)
                        .HasColumnType("decimal(18,2)");

            builder.HasMany(o => o.OrderItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.ApplicationUser)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
