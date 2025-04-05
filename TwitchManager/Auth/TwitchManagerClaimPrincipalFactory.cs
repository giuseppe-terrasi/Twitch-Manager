using System.Security.Claims;

namespace TwitchManager.Auth
{
    public class TwitchManagerClaimPrincipalFactory
    {
        public static ClaimsPrincipal CreatePrincipal(string accessToken, string twitchId, string userId, string username, string profileImageUrl, bool isAmin, IEnumerable<string> scopes = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "TwitchManager"),
                new("access_token", accessToken),
                new("userId", userId),
                new("twitchId", twitchId),
                new("username", username),
                new("profileImageUrl", profileImageUrl),
            };

            if(isAmin)
            {
                claims.Add(new(ClaimTypes.Role, "Admin"));
            }

            if (scopes != null)
            {
                foreach (var scope in scopes)
                {
                    claims.Add(new(ClaimTypes.Role, scope));
                }
            }

            var identity = new ClaimsIdentity(claims, TwitchManagerAuthenticationOptions.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}
