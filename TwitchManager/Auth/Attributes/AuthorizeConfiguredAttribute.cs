using Microsoft.AspNetCore.Authorization;

namespace TwitchManager.Auth.Attributes
{
    public class AuthorizeConfiguredAttribute : AuthorizeAttribute
    {
        public AuthorizeConfiguredAttribute()
        {
            Policy = TwitchManagerAuthenticationOptions.ConfiguredPolicy;
        }
    }
}
