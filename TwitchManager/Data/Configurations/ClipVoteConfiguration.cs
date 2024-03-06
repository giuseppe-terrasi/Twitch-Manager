using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class ClipVoteConfiguration : IEntityTypeConfiguration<ClipVote>
    {
        public void Configure(EntityTypeBuilder<ClipVote> builder)
        {
            builder.ToTable("ClipVotes");
            builder.HasKey(x => x.Id);
            builder.HasOne(c => c.Clip)
                .WithMany(c => c.ClipVotes)
                .HasForeignKey(c => c.ClipId);
            builder.HasOne(u => u.User)
                .WithMany(u => u.ClipVotes)
                .HasForeignKey(u => u.UserId);
        }
    }
}
