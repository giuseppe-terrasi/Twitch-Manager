using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Models.General;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Controllers
{
    [Route("webhooks")]
    public partial class WebhookController(ILogger<WebhookController> logger, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, 
        IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, IOptionsMonitor<ConfigData> optionsMonitor) : ControllerBase
    {
        private const string TwitchEventSubMessageId = "Twitch-Eventsub-Message-Id";
        private const string TwitchEventSubMessageType = "Twitch-Eventsub-Message-Type";
        private const string TwitchEventSubMessageSignature = "Twitch-Eventsub-Message-Signature";
        private const string TwitchEventSubMessageTimestamp = "Twitch-Eventsub-Message-Timestamp";

        /*
        private const string TwitchEventSubMessageRetry = "Twitch-Eventsub-Message-Retry";
        private const string TwitchEventSubSubscriptionType = "Twitch-Eventsub-Subscription-Type";
        private const string TwitchEventSubSubscriptionVersion = "Twitch-Eventsub-Subscription-Version";
        */

        private const string TwitchEventSubChallenge = "webhook_callback_verification";
        private const string TwitchEventSubNotification = "notification";

        private async Task<EventSub> GetEventSubAsync(JsonNode notification)
        {
            var twitchId = notification["subscription"]["id"].ToString();
            var dbContext = await dbContextFactory.CreateDbContextAsync(HttpContext.RequestAborted);

            var eventSub = await dbContext.EventSubs.Where(e => e.TwitchEventId == twitchId).FirstOrDefaultAsync(HttpContext.RequestAborted);

            return eventSub;
        }

        private static bool VerifyRequest(HttpRequest httpRequest, string body, string secret)
        {
            if(string.IsNullOrEmpty(secret))
            {
                return false;
            }

            var signature = httpRequest.Headers[TwitchEventSubMessageSignature];
            var hmacMessage = $"{httpRequest.Headers[TwitchEventSubMessageId]}{httpRequest.Headers[TwitchEventSubMessageTimestamp]}{body}";
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(hmacMessage));
            var toTest = $"sha256={BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()}";

            return signature == toTest;
        }

        private static ContentResult HandleChallenge(JsonNode notification)
        {
            var challenge = notification["challenge"].ToString();

            return new ContentResult
            {
                Content = challenge,
                ContentType = "text/plain",
                StatusCode = 200
            };
        }


    }
}
