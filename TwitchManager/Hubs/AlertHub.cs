using Microsoft.AspNetCore.SignalR;

namespace TwitchManager.Hubs
{
    public class AlertHub : Hub
    {

        public async Task JoinAlerts(string streamerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, streamerId);
        }
    }
}
