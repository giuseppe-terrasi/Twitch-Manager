using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class ClipConfiguration : IEntityTypeConfiguration<Clip>
    {
        public void Configure(EntityTypeBuilder<Clip> builder)
        {
            builder.ToTable("Clips");
            builder.HasKey(c => c.Id);
            builder.HasOne(c => c.Game)
                .WithMany(g => g.Clips)
                .HasForeignKey(c => c.GameId);
            builder.HasOne(c => c.Streamer)
                .WithMany(g => g.Clips)
                .HasForeignKey(c => c.BroadcasterId);
        }
    }
}
