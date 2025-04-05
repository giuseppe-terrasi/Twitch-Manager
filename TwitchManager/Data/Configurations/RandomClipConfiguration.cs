using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class RandomClipConfiguration : IEntityTypeConfiguration<RandomClip>
    {
        public void Configure(EntityTypeBuilder<RandomClip> builder)
        {
            builder.ToView("RandomClips");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Game).WithMany().HasForeignKey(x => x.GameId);
            builder.HasOne(x => x.Streamer).WithMany().HasForeignKey(x => x.BroadcasterId);
            builder.HasMany(x => x.ClipVotes).WithOne().HasForeignKey(x => x.ClipId);
        }
    }
}
