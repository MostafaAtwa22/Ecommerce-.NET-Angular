using Ecommerce.Core.Entities.Chat;
using Ecommerce.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ConversationConfigurations : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasQueryFilter(c => c.IsDeleted == false);

            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId);

            builder.HasMany(c => c.Participants)
                .WithOne(p => p.Conversation)
                .HasForeignKey(p => p.ConversationId);

            builder.Property(c => c.Status)
                .HasConversion(
                    x => x.ToString(),
                    x => (ConversationStatus) Enum.Parse(typeof(ConversationStatus), x)
                );
        }
    }
}