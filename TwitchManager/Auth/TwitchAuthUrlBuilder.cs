using Microsoft.Extensions.Options;

using TwitchManager.Models.General;

namespace TwitchManager.Auth
{
    public static class TwitchScopes
    { 
        public const string ModeratorReadFollowers = "moderation:read:followers";
        public const string ChannelReadSubscriptions = "channel:read:subscriptions";
    }

    public class TwitchAuthUrlBuilder(IOptionsMonitor<ConfigData> OptionsMonitor, IHttpContextAccessor httpContextAccessor)
    {
        public string BuildAuthUrl(IEnumerable<string> scopes = null)
        {
            var baseUrl = $"https://{httpContextAccessor.HttpContext.Request.Host}";

            var url = $"{OptionsMonitor.CurrentValue.TokenUrl}/authorize?response_type=code&client_id={OptionsMonitor.CurrentValue.ClientId}&redirect_uri={baseUrl}/TwitchAuthentication";

            if (scopes != null)
            {
                url += $"&scope={string.Join(" ", scopes)}";
            }

            return url;
        }
    }
}
