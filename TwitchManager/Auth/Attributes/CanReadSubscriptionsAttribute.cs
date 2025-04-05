using Microsoft.AspNetCore.Authorization;

namespace TwitchManager.Auth.Attributes
{
    public class CanReadSubscriptionsAttribute : AuthorizeAttribute
    {
        public CanReadSubscriptionsAttribute()
        {
            Roles = TwitchScopes.ChannelReadSubscriptions;
        }
    }
}
