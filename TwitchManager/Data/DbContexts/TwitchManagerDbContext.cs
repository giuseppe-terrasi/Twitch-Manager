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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
