using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class DiscordChannelConfiguration : IEntityTypeConfiguration<DiscordChannel>
    {
        public void Configure(EntityTypeBuilder<DiscordChannel> builder)
        {
            builder.ToTable("DiscordChannels");

            builder.HasKey(e => e.Id);
            builder.HasOne(x => x.Streamer)
                .WithMany(x => x.DiscordChannels)
                .HasForeignKey(e => e.StreamerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.ChannelId)
                .IsRequired();

            builder.Property(e => e.StreamerId)
                .IsRequired();
        }
    }
}
