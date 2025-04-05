using Microsoft.EntityFrameworkCore;

using System.Reflection;
using TwitchManager.Data.Domains;

namespace TwitchManager.Data.DbContexts
{
    public class TwitchManagerDbContext() : DbContext()
    {
        public DbSet<Clip> Clips { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Streamer> Streamers { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<BotUser> BotUsers { get; set; }

        public DbSet<ClipVote> ClipVotes { get; set; }

        public DbSet<UserStreamer> UserStreamers { get; set; }

        public DbSet<RandomClip> RandomClips { get; set; }

        public DbSet<Chat> Chats { get; set; }

        public DbSet<EventSub> EventSubs { get; set; }

        public DbSet<StreamerVotedGame> StreamerVotedGames { get; set; }

        public DbSet<TelegramChat> TelegramChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
