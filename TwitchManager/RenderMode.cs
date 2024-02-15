using Microsoft.AspNetCore.Components.Web;

namespace TwitchManager
{
    public static class RenderMode
    {
        public static InteractiveServerRenderMode InteractiveServerNotPrerendered { get; } = new(false);
    }
}
