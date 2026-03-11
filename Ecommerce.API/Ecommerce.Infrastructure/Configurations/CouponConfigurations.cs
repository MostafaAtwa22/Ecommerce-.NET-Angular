using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class CouponConfigurations : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.Property(c => c.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(c => c.Code).IsUnique();
        }
    }
}
