using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using TwitchManager.Models.General;

namespace TwitchManager.Data.DbContexts
{
    public class SqlLiteTwitchManagerDbContext(IOptionsMonitor<ConfigData> optionsMonitor) : TwitchManagerDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlite(optionsMonitor.CurrentValue.DbConnectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging(true);
        }
    }
}
