using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class LiveDataOptionConfiguration : IEntityTypeConfiguration<LiveDataOption>
    {
        public void Configure(EntityTypeBuilder<LiveDataOption> builder)
        {
            builder.ToTable("LiveDataOptions");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();
            builder.HasOne(o => o.Streamer)
                .WithMany()
                .HasForeignKey(o => o.StreamerId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
