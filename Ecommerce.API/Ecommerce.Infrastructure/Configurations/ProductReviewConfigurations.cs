using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ProductReviewConfigurations : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.Property(r => r.Rating)
                .HasColumnType("decimal(2,1)");

            builder.HasIndex(r => new {r.ApplicationUserId, r.ProductId})
                .IsUnique();

            builder.ToTable("ProductReviews");
        }
    }
}
