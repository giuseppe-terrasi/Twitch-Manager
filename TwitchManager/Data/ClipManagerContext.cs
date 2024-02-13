using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Reflection;

using TwitchManager.Data.Domains;
using TwitchManager.Models.General;

namespace TwitchManager.Data
{
    public class ClipManagerContext(IOptionsMonitor<ConfigData> optionsMonitor) 
        : DbContext(new DbContextOptionsBuilder().UseSqlite(optionsMonitor.CurrentValue.DbConnectionString).Options)
    {
        public DbSet<Clip> Clips { get; set; }

        public DbSet<Game> Games { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //SQLitePCL.Batteries.Init();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
