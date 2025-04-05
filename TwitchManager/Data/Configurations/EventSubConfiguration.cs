using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class EventSubConfiguration : IEntityTypeConfiguration<EventSub>
    {
        public void Configure(EntityTypeBuilder<EventSub> builder)
        {
            builder.ToTable("EventSubs");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.Type).IsRequired();
            builder.Property(e => e.Version).IsRequired();
            builder.Property(e => e.Condition).IsRequired()
                .HasColumnType("json");

            builder.HasOne(e => e.Streamer).WithMany(s => s.EventSubs)
                .HasForeignKey(e => e.StreamerId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Property(e => e.StreamerId).IsRequired();
            builder.Property(e => e.Status).HasConversion<string>().IsRequired();
            builder.Property(e => e.Action)
                .HasColumnType("json");

        }
    }
}
