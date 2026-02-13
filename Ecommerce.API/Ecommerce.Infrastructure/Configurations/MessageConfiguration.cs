using Ecommerce.Core.Entities.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Content)
                .IsRequired();

            builder.Property(m => m.IsRead)
                .HasDefaultValue(false);

            builder.Property(m => m.IsReceived)
                .HasDefaultValue(false);

            builder.Property(m => m.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(m => m.SenderId)
                .IsRequired();

            builder.Property(m => m.ReciverId)
                .IsRequired();

            // Configure relationships
            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(m => m.Reciver)
                .WithMany()
                .HasForeignKey(m => m.ReciverId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasQueryFilter(x => x.IsDeleted == false);
        }
    }
}
