using Microsoft.AspNetCore.SignalR;

using TwitchManager.Services.Abstractions;

namespace TwitchManager.Hubs
{
    public class ClipHub(IClipService clipService) : Hub
    {

        public async Task JoinLoop(string channelId)
        {
            channelId = "136110155";
            if (string.IsNullOrEmpty(channelId))
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }

        public async Task Vote(string clipId, string userId)
        {
            userId = userId.Replace("U", "");

            if (string.IsNullOrEmpty(clipId) || string.IsNullOrEmpty(userId))
                return;

            await clipService.VoteAsync(clipId, userId);
        }
    }
}
