using Ecommerce.Core.Entities.Chat;
using Ecommerce.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ChatParticipantConfigurations : IEntityTypeConfiguration<ChatParticipant>
    {
        public void Configure(EntityTypeBuilder<ChatParticipant> builder)
        {
            builder.HasQueryFilter(c => c.IsDeleted == false);

            builder.Property(c => c.Role)
                .HasConversion(
                    x => x.ToString(),
                    x => (ParticipantRole) Enum.Parse(typeof(ParticipantRole), x)
                );
        }
    }
}