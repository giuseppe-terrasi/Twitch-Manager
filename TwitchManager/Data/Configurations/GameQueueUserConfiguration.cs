using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class GameQueueUserConfiguration : IEntityTypeConfiguration<GameQueueUser>
    {
        public void Configure(EntityTypeBuilder<GameQueueUser> builder)
        {
            builder.ToTable("GameQueueUsers");
            builder.HasKey(gqu => gqu.Id);
            builder.Property(gqu => gqu.Username)
                .IsRequired();
            builder.Property(gqu => gqu.GameQueueId)
                .IsRequired();

            builder.HasOne(gqu => gqu.GameQueue)
                .WithMany(gq => gq.Users)
                .HasForeignKey(gqu => gqu.GameQueueId)
                .HasPrincipalKey(gq => gq.Id);
        }
    }
}
