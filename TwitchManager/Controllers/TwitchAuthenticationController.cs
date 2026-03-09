using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

using TwitchManager.Auth;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Models.Api.Clips.Responses;
using TwitchManager.Models.General;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Controllers
{
    [Route("[controller]")]
    public class TwitchAuthenticationController(IOptionsMonitor<ConfigData> optionsMonitor, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, 
        IStreamerService streamerService, IAuthenticationService authenticationService, TwitchAuthUrlBuilder twitchAuthUrlBuilder) : Controller
    {
        [HttpGet("/login")]
        public void Login([FromQuery]string returnUrl)
        {
            var url = twitchAuthUrlBuilder.BuildAuthUrl();

            Response.Cookies.Append("returnUrl", returnUrl, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            Response.Redirect(url);
        }


        [HttpGet]
        public async Task<IActionResult> Login([FromQuery(Name = "code")] string code, [FromQuery(Name = "error")] string error)
        {
            if(!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                return BadRequest(error);
            }

            var httpClient = new HttpClient();

            var baseUrl = $"https://{HttpContext.Request.Host}";

            var accesTokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{optionsMonitor.CurrentValue.TokenUrl}/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = optionsMonitor.CurrentValue.ClientId,
                    ["client_secret"] = optionsMonitor.CurrentValue.ClientSecret,
                    ["code"] = code,
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = $"{baseUrl}/TwitchAuthentication"
                })
            };

            var response = await httpClient.SendAsync(accesTokenRequest);

            if(!response.IsSuccessStatusCode)
                return BadRequest("Failed to get access token");

            var responseContent = await response.Content.ReadAsStringAsync();

            var twitchToken = JsonSerializer.Deserialize<TwitchToken>(responseContent);

            var userRequest = new HttpRequestMessage(HttpMethod.Get, $"{optionsMonitor.CurrentValue.BaseUrl}users");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", twitchToken.AccessToken);
            userRequest.Headers.Add("Client-Id", optionsMonitor.CurrentValue.ClientId);

            var userResponse = await httpClient.SendAsync(userRequest);

            if(!userResponse.IsSuccessStatusCode)
                return BadRequest("Failed to get user");

            var userResponseContent = await userResponse.Content.ReadAsStringAsync();
            var twitchUser = JsonSerializer.Deserialize<StreamerDataResponseModel>(userResponseContent)?.Data.FirstOrDefault();

            var dbContext = await dbContextFactory.CreateDbContextAsync();
            var existingUser = await dbContext.Users.Where(u => u.TwitchId == twitchUser.Id).FirstOrDefaultAsync();

            if(existingUser == null)
            {
                existingUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    TwitchId = twitchUser.Id,
                    CreatedOn = DateTime.UtcNow,
                };

                await dbContext.Users.AddAsync(existingUser);
                await dbContext.SaveChangesAsync();
            }

            var isAdmin = optionsMonitor.CurrentValue.AdminUsers.Contains(twitchUser.Id);

            var principal = TwitchManagerClaimPrincipalFactory.CreatePrincipal(twitchToken.AccessToken, twitchUser.Id, existingUser.Id, twitchUser.DisplayName, twitchUser.ProfileImageUrl, isAdmin);

            await HttpContext.SignInAsync(TwitchManagerAuthenticationOptions.AuthenticationScheme, principal);

            var redirectUrl = "";

            if(!string.IsNullOrEmpty(Request.Cookies["returnUrl"]))
            {
                redirectUrl = Request.Cookies["returnUrl"];
                Response.Cookies.Delete("returnUrl");

                return Redirect(redirectUrl);
            }

            if (await streamerService.UserHasAnyStreamer())
            {
                redirectUrl = "/clips";
            }
            else
            {
                redirectUrl = "/streamers";
            }

            return Redirect(redirectUrl);
        }

        [HttpGet("bot-authorize")]
        [Authorize(AuthenticationSchemes = TwitchManagerAuthenticationOptions.BotAuthenticationScheme)]
        public async Task<IActionResult> BotAuthorize()
        {
            using var dbcontext = await dbContextFactory.CreateDbContextAsync();

            var result = await authenticationService.AuthenticateAsync(HttpContext, TwitchManagerAuthenticationOptions.BotAuthenticationScheme);
            var twitchId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).FirstOrDefault();

            var botUser = await dbcontext.BotUsers.Where(u => u.TwitchId == twitchId).FirstOrDefaultAsync();

            if (botUser == null)
            {
                botUser = new BotUser
                {
                    Id = Guid.NewGuid().ToString(),
                    TwitchId = twitchId,
                    CreatedOn = DateTime.UtcNow,
                };
                dbcontext.BotUsers.Add(botUser);
            }

            botUser.AccessToken = result.Properties.GetTokenValue("access_token");
            botUser.RefreshToken = result.Properties.GetTokenValue("refresh_token");
            botUser.ExpirationDate = DateTime.Parse(result.Properties.GetTokenValue("expires_at"));
            botUser.IdToken = result.Properties.GetTokenValue("id_token");
            botUser.TwitchUsername = User.Claims.Where(c => c.Type == "preferred_username").Select(c => c.Value).FirstOrDefault();
            botUser.UpdatedOn = DateTime.UtcNow;

            await dbcontext.SaveChangesAsync();

            return Ok("ok");
        }

        private class TwitchToken
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("scope")]
            public string[] Scope { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
        }

    }
}