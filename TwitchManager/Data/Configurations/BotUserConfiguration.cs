using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TwitchManager.Data.Domains;

namespace TwitchManager.Data.Configurations
{
    public class BotUserConfiguration : IEntityTypeConfiguration<BotUser>
    {
        public void Configure(EntityTypeBuilder<BotUser> builder)
        {
            builder.ToTable("BotUsers");
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.TwitchId);
        }
    }
}
