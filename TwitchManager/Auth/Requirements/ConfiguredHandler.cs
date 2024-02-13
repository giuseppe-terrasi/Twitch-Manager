using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using TwitchManager.Models.General;

namespace TwitchManager.Auth.Requirements
{
    public class ConfiguredHandler(IOptionsMonitor<ConfigData> optionsMonitor) : AuthorizationHandler<ConfiguredRequirement>
    {
        private readonly IOptionsMonitor<ConfigData> _optionsMonitor = optionsMonitor;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ConfiguredRequirement requirement)
        {
            var isConfigued = _optionsMonitor.CurrentValue.IsConfigured();  

            if(isConfigued)
            {
                context.Succeed(requirement);
            }   

            return Task.CompletedTask;
        }
    }
}
