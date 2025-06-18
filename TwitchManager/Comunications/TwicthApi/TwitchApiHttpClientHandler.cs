using System.Text.Json;

using TwitchManager.Helpers;
using TwitchManager.Models.General;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Comunications.TwicthApi
{
    public class TwitchApiHttpClientHandler(ILogger<TwitchApiHttpClientHandler> logger, IWritableOptions<ConfigData> writableOptions, IAppTokenSerivice appTokenSerivice) : HttpClientHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var task = SendAsync(request, cancellationToken);
            task.Wait(cancellationToken);
            return task.Result;
        }

        private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(writableOptions.Value.Token) || writableOptions.Value.TokenExpiration < DateTime.Now)
            {
                var url = writableOptions.Value.TokenUrl + "/token";
                logger.LogInformation("Requesting new token from {Url}", url);
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(url, UriKind.Absolute))
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"client_id", writableOptions.Value.ClientId},
                        {"client_secret", writableOptions.Value.ClientSecret},
                        {"grant_type", "client_credentials"}
                    })
                };

                var response = await base.SendAsync(tokenRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to get token");

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                var tokenResponse = JsonSerializer.Deserialize<TwitchTokenResponse>(responseContent);

                writableOptions.Update(o =>
                {
                    o.Token = tokenResponse.AccessToken;
                    o.TokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                });
            }

            return writableOptions.Value.Token;
        }

        private async Task AddHeadersAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Client-ID", writableOptions.Value.ClientId);
            request.Headers.Add("Authorization", $"Bearer {await GetTokenAsync(cancellationToken)}");
        }

        private async Task AddBotHeadersAsync(TwitchApiHttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Client-ID", writableOptions.Value.ClientId);
            request.Headers.Add("Authorization", $"Bearer {await appTokenSerivice.GetAccessTokenAsync(request.BotUserId, cancellationToken)}");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request is not TwitchApiHttpRequestMessage twitchApiHttpRequestMessage)
                throw new ArgumentException("Request must be of type TwitchApiHttpRequestMessage", nameof(request));

            if(!string.IsNullOrEmpty(twitchApiHttpRequestMessage.BotUserId))
            {
                await AddBotHeadersAsync(twitchApiHttpRequestMessage, cancellationToken);
            }
            else if(twitchApiHttpRequestMessage.AuthorizationRequired)
            {
                await AddHeadersAsync(twitchApiHttpRequestMessage, cancellationToken);
            }

            return await base.SendAsync(twitchApiHttpRequestMessage, cancellationToken);
        }
    }
}
