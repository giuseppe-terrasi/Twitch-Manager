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
using Microsoft.AspNetCore.Authorization;
using TwitchManager.Helpers;
using TwitchManager.Services.Abstractions;

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
                _ = HandleStreamOnlineNotification(notification);

                return NoContent();
            }

            logger.LogWarning("Received chat-message webhook notification with unknown message type: {type}", requestType);

            return Ok();
        }

        [HttpGet("/manual-stream-online")]
        [Authorize]
        public async Task<IActionResult> ManualStreamOnlineAsync(string streamerId)
        {
            var userId = HttpContext.User.GetUserTwitchId();
            var sreamerService = serviceProvider.GetRequiredService<IStreamerService>();
            var hostStreamerId = sreamerService.GetStreamerIdByHost();
            var isAdmin = HttpContext.User.IsInRole("Admin");

            if (userId != hostStreamerId && !isAdmin)
            {
                return Ok($"Utente {userId}: Non hai i permessi per invocare questa azione");
            }

            var result = await HandleStreamOnlineNotification(hostStreamerId);

            return Ok(result);
        }

        private async Task HandleStreamOnlineNotification(JsonNode notification)
        {
            var streamerId = notification["event"]["broadcaster_user_id"].ToString();

            await HandleStreamOnlineNotification(streamerId);
        }

        private async Task<string> HandleStreamOnlineNotification(string streamerId)
        {
            logger.LogInformation("Handling stream online notification");

            var twitchManagerDbContext = await dbContextFactory.CreateDbContextAsync();

            var twitchHttpClient = httpClientFactory.CreateClient("TwitchApi");

            logger.LogInformation("Streamer {streamerId} is live", streamerId);

            var streamer = await twitchManagerDbContext.Streamers
                .Include(s => s.TelegramChats)
                .Include(s => s.DiscordChannels)
                .FirstOrDefaultAsync(s => s.Id == streamerId);

            var request = new GetLiveInfoHttpRequestMessage(streamerId);
            var liveInfoResponse = await twitchHttpClient.SendAsync(request);

            if (!liveInfoResponse.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get live info for streamer {streamerId}: {statusCode}", streamerId, liveInfoResponse.StatusCode);
                return "Errore nel recupero dello stato della live";
            }

            var liveInfo = await request.GetDataAsync(liveInfoResponse);

            var liveTitle = liveInfo.Data.FirstOrDefault()?.Title;
            var thumbnailUrl = liveInfo.Data.FirstOrDefault()?.ThumbnailUrl ?? "";

            thumbnailUrl = thumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080");

            if(string.IsNullOrEmpty(thumbnailUrl))
            {
                logger.LogWarning("Failed to get thumbnail URL for streamer {streamerId}", streamerId);
                return "Nessun messaggio inviato, live non trovata";
            }

            var imageHttpClient = new HttpClient();

            var attempt = 0;
            byte[] thumbnail = null;

            while (attempt < 3)
            {
                try
                {
                    thumbnail = await imageHttpClient.GetByteArrayAsync(thumbnailUrl);

                    if(thumbnail?.Length > 0)
                    {
                        logger.LogInformation("Successfully downloaded thumbnail for streamer {streamerId}", streamerId);
                        break;
                    }
                    else
                    {
                        logger.LogWarning("Failed to download thumbnail for streamer {streamerId}", streamerId);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to get thumbnail for streamer {streamerId}", streamerId);
                }
                attempt++;
                await Task.Delay(1000);
            }

            if (thumbnail == null || thumbnail.Length == 0)
            {
                logger.LogWarning("Failed to download thumbnail for streamer {streamerId}", streamerId);
                return "Nessun messaggio inviato, live non trovata o thumbnail non disponibile";
            }

            var message = $"{streamer.DisplayName} è live!\r\n{liveTitle}\r\nhttps://www.twitch.tv/{streamer.DisplayName}";

            foreach (var discordChannel in streamer.DiscordChannels)
            {
                var channelId = discordChannel.ChannelId;
                if (channelId != null)
                {
                    await SendDiscordNotificationAsync(channelId, message, thumbnail).ConfigureAwait(false);
                }
            }

            foreach (var telegramChat in streamer.TelegramChats)
            {
                var chatId = telegramChat.ChatId;
                if (chatId != null)
                {
                    await SendTelegramNotificationAsync(chatId, message, thumbnail).ConfigureAwait(false);
                }
            }

            return "Messaggio inviato correttamente!";
        }

        private async Task SendTelegramNotificationAsync(string chatId, string message, byte[] thumbnail)
        {
            try
            {
                var telegramHttpClient = httpClientFactory.CreateClient("Telegram");

                var config = optionsMonitor.CurrentValue;

                var content = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(thumbnail);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var contentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "photo",
                    FileName = "image.jpg"
                };
                fileContent.Headers.ContentDisposition = contentDisposition;
                content.Add(fileContent);
                content.Add(new StringContent(chatId), "chat_id");
                content.Add(new StringContent(message), "caption");

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

        private async Task SendDiscordNotificationAsync(string channelId, string message, byte[] thumbnail)
        {
            try
            {
                var discordHttpClient = httpClientFactory.CreateClient("Discord");

                var config = optionsMonitor.CurrentValue;

                var content = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(thumbnail);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var contentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "files[0]",
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
