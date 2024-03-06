using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TwitchManager.Models.General;

namespace TwitchManager.Data.DbContexts
{
    public class TwitchManagerDbContextFactory(IOptionsMonitor<ConfigData> optionsMonitor) : IDbContextFactory<TwitchManagerDbContext>
    {
        public TwitchManagerDbContext CreateDbContext()
        {
            if (optionsMonitor.CurrentValue.ConfigType == ConfigType.StandAlone)
            {
                return new SqlLiteTwitchManagerDbContext(optionsMonitor);
            }

            return new MySqlTwitchManagerDbContext(optionsMonitor);
        }
    }
}
