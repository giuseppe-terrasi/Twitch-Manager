using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using System.Text.Json;

using TwitchManager.Data.Domains;
using System.Text.RegularExpressions;
using TwitchManager.Comunications.TwicthApi.Api.Messages;
using TwitchManager.Data.DbContexts;

namespace TwitchManager.Controllers
{
    public partial class WebhookController
    {
        [HttpPost("chat-message")]
        public async Task<IActionResult> ChatMessageAsync()
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();

            var notification = JsonSerializer.Deserialize<JsonNode>(body);
            var eventSub = await GetEventSubAsync(notification);

            if (!VerifyRequest(Request, body, eventSub?.Secret))
            {
                logger.LogWarning("Failed to verify chat-message webhook notification");
                return StatusCode(403);
            }

            var requestType = Request.Headers[TwitchEventSubMessageType];

            if (requestType == TwitchEventSubChallenge)
            {
                logger.LogInformation("Received chat-message webhook challenge");
                return HandleChallenge(notification);
            }

            if (requestType == TwitchEventSubNotification)
            {
                return await HandleChatNotification(notification);
            }

            logger.LogWarning("Received chat-message webhook notification with unknown message type: {type}", requestType);

            return Ok();
        }

        private async Task<NoContentResult> HandleChatNotification(JsonNode notification)
        {
            var twitchManagerDbContext = await dbContextFactory.CreateDbContextAsync();

            try
            {
                var eventSub = await GetEventSubAsync(notification);
                if(eventSub != null && eventSub.Status == EventSubStatus.Pending)
                {
                    eventSub.Status = EventSubStatus.Enabled;
                    await twitchManagerDbContext.EventSubs
                        .Where(e => e.Id == eventSub.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, eventSub.Status));
                }   

                var message = notification["event"]["message"]["text"].ToString();  

                var regex = CommandRegex();

                var match = regex.Match(message);

                if (match.Success) {
                    var command = match.Groups[1].Value;
                    var option = match.Groups[2].Value;

                    if(command == "vota")
                    {
                        await HandleVoteAsync(notification, option, twitchManagerDbContext);
                    }
                    else
                    {
                        logger.LogWarning("Unknown command received: {command}", command);
                    }
                }

                //var streamerId = notification["event"]["broadcaster_user_id"].ToString();

                //var existingStreamer = await twitchManagerDbContext.Streamers.FindAsync(streamerId);

                //if (existingStreamer == null)
                //{
                //    return NoContent();
                //}

                //var chatMessage = new Chat
                //{
                //    Id = Guid.NewGuid(),
                //    StreamerId = streamerId,
                //    RawData = notification.ToString()
                //};

                //await twitchManagerDbContext.Chats.AddAsync(chatMessage);

                //await twitchManagerDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle chat-message webhook notification");
            }
            return NoContent();
        }

        private async Task HandleVoteAsync(JsonNode notification, string vote, TwitchManagerDbContext twitchManagerDbContext)
        {
            var broadcasterId = notification["event"]["broadcaster_user_id"].ToString();
            var userId = notification["subscription"]["condition"]["user_id"].ToString();
            var chatterUsername = notification["event"]["chatter_user_name"].ToString();
            var chatterUserId = notification["event"]["chatter_user_id"].ToString();

            var client = httpClientFactory.CreateClient("TwitchApi");

            var isSubscriber = notification["event"]["badges"].AsArray().Any(b => b["set_id"].ToString() == "subscriber");

            var responseMessage = "";

            if(isSubscriber)
            {
                try
                {
                    var streamerVotedGame = await twitchManagerDbContext.StreamerVotedGames
                .Where(s => s.StreamerId == broadcasterId && s.ChatterUsername == chatterUsername)
                .FirstOrDefaultAsync();

                    if (streamerVotedGame == null)
                    {
                        streamerVotedGame = new StreamerVotedGame
                        {
                            Id = Guid.NewGuid().ToString(),
                            StreamerId = broadcasterId,
                            ChatterId = chatterUserId,
                            ChatterUsername = chatterUsername,
                        };
                        twitchManagerDbContext.StreamerVotedGames.Add(streamerVotedGame);
                    }

                    streamerVotedGame.GameName = vote;
                    streamerVotedGame.VotedOn = DateTime.UtcNow;

                    await twitchManagerDbContext.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Failed to handle vote");
                    return;
                }

                responseMessage = $"@{chatterUsername} Voto ricevuto: {vote}";
            }
            else
            {
                responseMessage = $"@{chatterUsername} Devi essere abbonato per votare";
            }

            var request = new SendMessageRequestMessage(broadcasterId, userId, responseMessage);

            var response = await client.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Ok wih content: {content}", content);
            }
            else
            {
                logger.LogWarning("Failed with content: {content}", content);
            }
        }

        [GeneratedRegex(@"^!(\w+)\s+(.+)")]
        private static partial Regex CommandRegex();
    }
}
