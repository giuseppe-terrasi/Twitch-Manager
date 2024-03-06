using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using TwitchManager.Models.General;

namespace TwitchManager.Data.DbContexts
{
    public class MySqlTwitchManagerDbContext(IOptionsMonitor<ConfigData> optionsMonitor) : TwitchManagerDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var connectionString = optionsMonitor.CurrentValue.DbConnectionString;
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            optionsBuilder
                .UseMySql(connectionString, serverVersion, o =>
                {
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .EnableSensitiveDataLogging(true);
        }
    }
}
