using System.Security.Claims;

namespace TwitchManager.Auth
{
    public class TwitchManagerClaimPrincipalFactory
    {
        public static ClaimsPrincipal CreatePrincipal(string accessToken, string userId, string username, string profileImageUrl, bool isAmin)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "TwitchManager"),
                new("access_token", accessToken),
                new("userId", userId),
                new("username", username),
                new("profileImageUrl", profileImageUrl),
            };

            if(isAmin)
            {
                claims.Add(new(ClaimTypes.Role, "Admin"));
            }

            var identity = new ClaimsIdentity(claims, TwitchManagerAuthenticationOptions.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}
