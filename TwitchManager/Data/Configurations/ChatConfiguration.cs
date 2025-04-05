using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("Chats");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever()
                .HasConversion<string>();

            builder.HasOne(c => c.Streamer)
                .WithMany(s => s.ChatMessages)
                .HasForeignKey(c => c.StreamerId)
                .HasPrincipalKey(s => s.Id);

            builder.Property(c => c.StreamerId).IsRequired();
            builder.Property(c => c.RawData).IsRequired()
                .HasColumnType("json");

        }
    }
}
