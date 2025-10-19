using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ProductConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.ProductBrand)
                    .WithMany(b => b.Products)
                    .HasForeignKey(p => p.ProductBrandId);

            builder.HasOne(p => p.ProductType)
                    .WithMany(t => t.Products)
                    .HasForeignKey(p => p.ProductTypeId);
        }
    }
}