using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Comunications.TwicthApi.Api.Streamers;

namespace TwitchManager.Controllers
{
    public partial class WebhookController
    {
        [HttpPost("stream-online")]
        public async Task<IActionResult> StreamOnlineAsync()
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();

            var notification = JsonSerializer.Deserialize<JsonNode>(body);
            var eventSub = await GetEventSubAsync(notification);

            if (!VerifyRequest(Request, body, eventSub?.Secret))
            {
                logger.LogWarning("Failed to verify stream-online webhook notification");
                return StatusCode(403);
            }

            var requestType = Request.Headers[TwitchEventSubMessageType];

            if (requestType == TwitchEventSubChallenge)
            {
                logger.LogInformation("Received stream-online webhook challenge");
                return HandleChallenge(notification);
            }

            if (requestType == TwitchEventSubNotification)
            {
                return await HandleStreamOnlineNotification(notification);
            }

            logger.LogWarning("Received chat-message webhook notification with unknown message type: {type}", requestType);

            return Ok();
        }

        private async Task<NoContentResult> HandleStreamOnlineNotification(JsonNode notification)
        {
            try
            {
                var twitchManagerDbContext = await dbContextFactory.CreateDbContextAsync();

                var streamerId = notification["event"]["broadcaster_user_id"].ToString();

                var telegramChats = await twitchManagerDbContext.TelegramChats
                    .Include(t => t.Streamer)
                    .Where(x => x.StreamerId == streamerId)
                    .FirstOrDefaultAsync();

                if(telegramChats == null)
                {
                    logger.LogWarning("No telegram chat found for streamer {streamerId}", streamerId);
                    return NoContent();
                }

                var telegramHttpClient = httpClientFactory.CreateClient("Telegram");
                var twitchHttpClient = httpClientFactory.CreateClient("TwitchApi");
                var request = new GetLiveInfoHttpRequestMessage(streamerId);
                var liveInfoResponse = await twitchHttpClient.SendAsync(request);

                if(!liveInfoResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to get live info for streamer {streamerId}: {statusCode}", streamerId, liveInfoResponse.StatusCode);
                    return NoContent();
                }

                var liveInfo = await request.GetDataAsync(liveInfoResponse);

                var liveTitle = liveInfo.Data.FirstOrDefault()?.Title;
                var thumbnailUrl = liveInfo.Data.FirstOrDefault()?.ThumbnailUrl ?? "";

                thumbnailUrl = thumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080");

                var message = $"{telegramChats.Streamer.DisplayName} è live!\r\n{liveTitle}\r\nhttps://www.twitch.tv/{telegramChats.Streamer.DisplayName}";

                var config = optionsMonitor.CurrentValue;

                var body = new JsonObject
                {
                    ["chat_id"] = telegramChats.ChatId,
                    ["caption"] = message,
                    ["photo"] = thumbnailUrl,
                };

                var json = body.ToString();

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await telegramHttpClient.PostAsync($"/bot{config.TelegramBotToken}/sendPhoto", content);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to send message to Telegram chat: {statusCode}", response.StatusCode);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle stream-online notification");
            }

            return NoContent();
        }
    }
}
