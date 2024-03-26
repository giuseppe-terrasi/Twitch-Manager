using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class UserStreamerConfiguration : IEntityTypeConfiguration<UserStreamer>
    {
        public void Configure(EntityTypeBuilder<UserStreamer> builder)
        {
            builder.ToTable("UserStreamers");
            builder.HasKey(us => us.Id);
            
            builder.HasOne(us => us.User)
                .WithMany(u => u.UserStreamers)
                .HasForeignKey(us => us.UserId);

            builder.HasOne(us => us.Streamer)
                .WithMany(s => s.UserStreamers)
                .HasForeignKey(us => us.StreamerId);
        }
    }
}
