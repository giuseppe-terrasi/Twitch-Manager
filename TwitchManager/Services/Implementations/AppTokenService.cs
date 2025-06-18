
using Microsoft.EntityFrameworkCore;

using System.Text.Json;
using System.Text.Json.Nodes;

using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Helpers;
using TwitchManager.Models.General;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class AppTokenService(ILogger<AppTokenService> logger, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IWritableOptions<ConfigData> writableOptions) : IAppTokenSerivice
    {
        private readonly SemaphoreSlim _semaphore = new(1);

        public async Task<string> GetAccessTokenAsync(string twitchId,  CancellationToken cancellationToken)
        {
            string token = "";

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var appToken = await dbContext.BotUsers
                    .Where(b => b.TwitchId == twitchId)
                    .FirstOrDefaultAsync(cancellationToken);

                var newTokenRequired = appToken == null || appToken.ExpirationDate < DateTime.UtcNow.AddMinutes(5);

                if (appToken == null)
                {
                    appToken = new BotUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        TwitchId = twitchId,
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
            var clientId = writableOptions.Value.ClientId;
            var clientSecret = writableOptions.Value.ClientSecret;
            var tokenUrl = writableOptions.Value.TokenUrl + "/token";

            var grantType = botUser.RefreshToken == null ? "client_credentials" : "refresh_token";

            var data = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", grantType }
            };

            if (botUser.RefreshToken != null)
            {
                data.Add("refresh_token", botUser.RefreshToken);
            }

            var content = new FormUrlEncodedContent(data);

            var client = new HttpClient();

            logger.LogInformation("Requesting new access token for Twitch user {TwitchId} using refresh token {RefreshToken} to url {url}", 
                botUser.TwitchId, botUser.RefreshToken, tokenUrl);

            var response = await client.PostAsync(tokenUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var token = JsonSerializer.Deserialize<JsonNode>(json);

                botUser.AccessToken = token["access_token"].ToString();
                botUser.ExpirationDate = DateTime.UtcNow.AddSeconds((int)token["expires_in"]);
                botUser.RefreshToken = token["refresh_token"].ToString();
            }
            else
            {
                logger.LogError("Failed to get access token from Twitch API. Status code: {StatusCode} using refresh token {refesh}", 
                    response.StatusCode, botUser.RefreshToken);

                throw new Exception("Failed to get access token");
            }
        }
    }
}
