using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
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
    public class TwitchAuthenticationController(IOptionsMonitor<ConfigData> optionsMonitor, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IStreamerService streamerService) : Controller
    {

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