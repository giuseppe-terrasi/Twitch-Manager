using Microsoft.EntityFrameworkCore;

using MySqlConnector;

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

        public DbSet<GameQueue> GameQueues { get; set; }

        public DbSet<GameQueueUser> GameQueueUsers { get; set; }

        public DbSet<LiveDataOption> LiveDataOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<RandomClip> GetRandomClips(string broadcasterId, bool? downloaded, int limit)
        {
            return RandomClips.FromSqlRaw("CALL twitchmanager.GetRandomClips(@BroadcasterId, @Downloaded, @Limit)", 
                new MySqlParameter("@BroadcasterId", broadcasterId),
                new MySqlParameter("@Downloaded", downloaded.HasValue ? downloaded.Value ? 1 : 0 : -1),
                new MySqlParameter("@Limit", limit))
                .AsNoTracking()
                .AsQueryable();
        }
    }
}
