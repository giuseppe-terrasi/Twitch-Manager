using System.Security.Claims;

namespace TwitchManager.Helpers
{
    public static class Extensions
    {
        public static string GetAccessToken(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
        }

        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        }

        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
        }

        public static string GetProfileImageUrl(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type == "profileImageUrl")?.Value;
        }
    }
}
