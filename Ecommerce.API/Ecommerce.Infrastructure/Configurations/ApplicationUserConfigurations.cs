using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ApplicationUserConfigurations : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.OwnsOne(u => u.Address, a =>
            {
                a.ToTable("Addresses");
                a.WithOwner()
                    .HasForeignKey("UserId");
                a.HasKey("UserId");

                a.Property(p => p.Country)
                    .HasMaxLength(100);
                a.Property(p => p.Government)
                    .HasMaxLength(100);
                a.Property(p => p.City)
                    .HasMaxLength(100);
                a.Property(p => p.Street)
                    .HasMaxLength(150);
                a.Property(p => p.Zipcode)
                    .HasMaxLength(20);
            });

            builder.Property(u => u.Gender)
                .HasConversion(
                    x => x.ToString(),
                    x => (Gender)Enum.Parse(typeof(Gender), x)
                );
        }
    }
}