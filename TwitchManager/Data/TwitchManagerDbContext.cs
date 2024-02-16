using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Reflection;

using TwitchManager.Data.Domains;
using TwitchManager.Models.General;

namespace TwitchManager.Data
{
    public class TwitchManagerDbContext(IOptionsMonitor<ConfigData> optionsMonitor) 
        : DbContext()
    {
        public DbSet<Clip> Clips { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Streamer> Streamers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .UseSqlite(optionsMonitor.CurrentValue.DbConnectionString)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging(true);
            //SQLitePCL.Batteries.Init();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
