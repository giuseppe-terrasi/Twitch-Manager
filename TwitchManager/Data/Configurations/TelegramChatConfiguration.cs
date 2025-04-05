using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class TelegramChatConfiguration : IEntityTypeConfiguration<TelegramChat>
    {
        public void Configure(EntityTypeBuilder<TelegramChat> builder)
        {
            builder.ToTable("TelegramChats");

            builder.HasKey(e => e.Id);
            builder.HasOne(x => x.Streamer)
                .WithMany()
                .HasForeignKey(e => e.StreamerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.ChatId)
                .IsRequired();

            builder.Property(e => e.StreamerId)
                .IsRequired();
        }
    }
}
