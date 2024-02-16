using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class StreamerConfiguration : IEntityTypeConfiguration<Streamer>
    {
        public void Configure(EntityTypeBuilder<Streamer> builder)
        {
            builder.ToTable("Streamers");
            builder.HasKey(c => c.Id);
        }
    }
}
