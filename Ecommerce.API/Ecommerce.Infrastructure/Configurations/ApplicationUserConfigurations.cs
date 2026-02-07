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
            builder.Property(u => u.Gender)
                .HasConversion(
                    x => x.ToString(),
                    x => (Gender)Enum.Parse(typeof(Gender), x)
                );
        }
    }
}