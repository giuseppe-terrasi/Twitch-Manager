using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class GameQueueConfiguration : IEntityTypeConfiguration<GameQueue>
    {
        public void Configure(EntityTypeBuilder<GameQueue> builder)
        {
            builder.ToTable("GameQueues");
            builder.HasKey(gq => gq.Id);
            builder.Property(gq => gq.StreamerId)
                .IsRequired();
            builder.Property(gq => gq.GameName);

            builder.Property(gq => gq.IsOpen)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}
