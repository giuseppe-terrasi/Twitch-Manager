using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

using System.Security.Claims;
using System.Text.Encodings.Web;

using TwitchManager.Models.General;

namespace TwitchManager.Auth
{
    public class TwitchManagerAuthenticationHandler(IOptionsMonitor<TwitchManagerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, IOptionsMonitor<ConfigData> optionsMonitor) 
        : AuthenticationHandler<TwitchManagerAuthenticationOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(optionsMonitor.CurrentValue == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("No configuration found"));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "TwitchManager")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
