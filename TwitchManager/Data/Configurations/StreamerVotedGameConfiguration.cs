using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class StreamerVotedGameConfiguration : IEntityTypeConfiguration<StreamerVotedGame>
    {
        public void Configure(EntityTypeBuilder<StreamerVotedGame> builder)
        {
            builder.ToTable("StreamerVotedGames");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.StreamerId);

        }
    }
}
