using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Comunications.TwicthApi.Api.Streamers;
using TwitchManager.Data.Domains;
using System.Net.Http;
using System.Net.Http.Headers;

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
            var twitchManagerDbContext = await dbContextFactory.CreateDbContextAsync();

            var twitchHttpClient = httpClientFactory.CreateClient("TwitchApi");

            var streamerId = notification["event"]["broadcaster_user_id"].ToString();

            var streamer = await twitchManagerDbContext.Streamers
                .Include(s => s.TelegramChats)
                .Include(s => s.DiscordChannels)
                .FirstOrDefaultAsync(s => s.Id == streamerId);

            var request = new GetLiveInfoHttpRequestMessage(streamerId);
            var liveInfoResponse = await twitchHttpClient.SendAsync(request);

            if (!liveInfoResponse.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get live info for streamer {streamerId}: {statusCode}", streamerId, liveInfoResponse.StatusCode);
                return NoContent();
            }

            var liveInfo = await request.GetDataAsync(liveInfoResponse);

            var liveTitle = liveInfo.Data.FirstOrDefault()?.Title;
            var thumbnailUrl = liveInfo.Data.FirstOrDefault()?.ThumbnailUrl ?? "";

            thumbnailUrl = thumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080");
            var message = $"{streamer.DisplayName} è live!\r\n{liveTitle}\r\nhttps://www.twitch.tv/{streamer.DisplayName}";

            foreach (var discordChannel in streamer.DiscordChannels)
            {
                var channelId = discordChannel.ChannelId;
                if (channelId != null)
                {
                    await SendDiscordNotificationAsync(channelId, message, thumbnailUrl).ConfigureAwait(false);
                }
            }

            foreach (var telegramChat in streamer.TelegramChats)
            {
                var chatId = telegramChat.ChatId;
                if (chatId != null)
                {
                    await SendTelegramNotificationAsync(chatId, message, thumbnailUrl).ConfigureAwait(false);
                }
            }

            return NoContent();
        }

        private async Task SendTelegramNotificationAsync(string chatId, string message, string thumbnailUrl)
        {
            try
            {
                var telegramHttpClient = httpClientFactory.CreateClient("Telegram");

                var config = optionsMonitor.CurrentValue;

                var body = new JsonObject
                {
                    ["chat_id"] = chatId,
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
        }

        private async Task SendDiscordNotificationAsync(string channelId, string message, string thumbnailUrl)
        {
            try
            {
                var discordHttpClient = httpClientFactory.CreateClient("Discord");

                var config = optionsMonitor.CurrentValue;

                var content = new MultipartFormDataContent();

                var imageHttpClient = new HttpClient();

                var fileContent = new ByteArrayContent(await imageHttpClient.GetByteArrayAsync(thumbnailUrl));

                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var contentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = $"\"files[0]\"",
                    FileName = "image.jpg"
                };
                fileContent.Headers.ContentDisposition = contentDisposition;

                content.Add(fileContent);

                var discordMessage = $"@everyone {message}";
                content.Add(new StringContent(discordMessage), "content");


                var response = await discordHttpClient.PostAsync($"api/channels/{channelId}/messages", content);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to handle stream-online notification");
            }
        }
    }
}
