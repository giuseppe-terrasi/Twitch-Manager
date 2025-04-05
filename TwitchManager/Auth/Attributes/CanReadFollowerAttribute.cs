using Microsoft.AspNetCore.Authorization;

namespace TwitchManager.Auth.Attributes
{
    public class CanReadFollowerAttribute : AuthorizeAttribute
    {
        public CanReadFollowerAttribute()
        {
            Roles = TwitchScopes.ModeratorReadFollowers;
        }
    }
}
