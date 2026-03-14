
namespace Ecommerce.Infrastructure.Configurations
{
        public class ProductConfigurations : IEntityTypeConfiguration<Product>
        {
                public void Configure(EntityTypeBuilder<Product> builder)
                {
                        builder.Property(p => p.Price)
                                .HasColumnType("decimal(18,2)");

                        builder.OwnsOne(p => p.Discount, d =>
                        {
                                d.Property(pd => pd.Percentage)
                                        .HasColumnName("DiscountPercentage")
                                        .HasColumnType("decimal(5,2)");
                                d.Property(pd => pd.Name)
                                        .HasColumnName("DiscountName");
                                d.Property(pd => pd.ExpirationDate)
                                        .HasColumnName("DiscountExpirationDate");
                        });

                        builder.Property(p => p.AverageRating)
                                .HasColumnType("decimal(5,2)");

                        builder.HasOne(p => p.ProductBrand)
                                .WithMany(b => b.Products)
                                .HasForeignKey(p => p.ProductBrandId);

                        builder.HasOne(p => p.ProductType)
                                .WithMany(t => t.Products)
                                .HasForeignKey(p => p.ProductTypeId);

                        builder.HasMany(p => p.ProductReviews)
                                .WithOne(r => r.Product)
                                .HasForeignKey(r => r.ProductId)
                                .OnDelete(DeleteBehavior.Cascade);
                }
        }
}
