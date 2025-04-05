
using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Text.Json.Nodes;

using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class AppTokenSerivice(ILogger<AppTokenSerivice> logger, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IConfiguration configuration) : IAppTokenSerivice
    {
        private readonly SemaphoreSlim _semaphore = new(1);

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            string token = "";

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var appToken = await dbContext.BotUsers
                    .Where(b => b.TwitchId == "app")
                    .FirstOrDefaultAsync(cancellationToken);

                var newTokenRequired = appToken == null || appToken.ExpirationDate < DateTime.UtcNow;

                if (appToken == null)
                {
                    appToken = new BotUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        TwitchId = "app",
                    };

                    dbContext.BotUsers.Add(appToken);
                }

                if (newTokenRequired)
                {
                    await RequestAccessTokenAsync(appToken, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                token = appToken.AccessToken;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get access token");
            }
            finally
            {
                _semaphore.Release();
            }

            return token;
        }

        private async Task RequestAccessTokenAsync(BotUser botUser, CancellationToken cancellationToken)
        {
            var clientId = configuration["Twitch:ClientId"];
            var clientSecret = configuration["Twitch:ClientSecret"];
            var tokenUrl = configuration["Twitch:TokenUrl"];
            
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "client_credentials" },
            });

            var client = new HttpClient();

            var response = await client.PostAsync(tokenUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var token = JsonSerializer.Deserialize<JsonNode>(json);

                botUser.AccessToken = token["access_token"].ToString();
                botUser.ExpirationDate = DateTime.UtcNow.AddSeconds((int)token["expires_in"]);
                botUser.RefreshToken = "";
            }
            else
            {
                throw new Exception("Failed to get access token");
            }
        }
    }
}
