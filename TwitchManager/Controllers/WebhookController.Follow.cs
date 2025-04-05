using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using System.Text.Json;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using Microsoft.AspNetCore.SignalR;
using TwitchManager.Hubs;

namespace TwitchManager.Controllers
{
    public partial class WebhookController
    {
        [HttpPost("channel-follow")]
        public async Task<IActionResult> ChannelFollowAsync()
        {
            logger.LogInformation("Received channel-follow webhook notification");

            var body = await new StreamReader(Request.Body).ReadToEndAsync();
            var notification = JsonSerializer.Deserialize<JsonNode>(body);
            var eventSub = await GetEventSubAsync(notification);

            if (!VerifyRequest(Request, body, eventSub?.Secret))
            {
                logger.LogWarning("Failed to verify channel-follow webhook notification");
                return StatusCode(403);
            }

            var requestType = Request.Headers[TwitchEventSubMessageType];

            if (requestType == TwitchEventSubChallenge)
            {
                logger.LogInformation("Received channel-follow webhook challenge");
                return HandleChallenge(notification);
            }

            if (requestType == TwitchEventSubNotification)
            {
                logger.LogInformation("Received channel-follow webhook notification");
                return await HandleFollowNotification(notification, eventSub);
            }

            logger.LogWarning("Received channel-follow webhook notification with unknown message type: {type}", requestType);

            return Ok();
        }

        private async Task<NoContentResult> HandleFollowNotification(JsonNode notification, EventSub eventSub)
        {
            try
            {
                var hubContext = serviceProvider.GetRequiredService<IHubContext<AlertHub>>();

                var streamerId = eventSub.StreamerId;

                var follow = new
                {
                    StreamerId = streamerId,
                    UserId = notification["event"]["user_id"].ToString(),
                    FollowedAt = DateTime.Parse(notification["event"]["followed_at"].ToString())
                };

                await hubContext.Clients.Group(streamerId).SendAsync("Follow", follow);

                logger.LogInformation("Handled channel-follow notification: {data}", notification);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to handle channel-follow notification: {data}", notification);
            }
            

            return NoContent();
        }
    }
}
