using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using TwitchManager.Components.Pages.Streamers;
using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Comunications.TwicthApi.Api.Messages;
using TwitchManager.Comunications.TwicthApi.Api.Streamers;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TwitchManager.Controllers
{
    public partial class WebhookController
    {
        [HttpPost("chat-message")]
        public async Task<IActionResult> ChatMessageAsync()
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();

            var notification = JsonSerializer.Deserialize<JsonNode>(body);
            var eventSub = await GetEventSubAsync(notification);

            if (!VerifyRequest(Request, body, eventSub?.Secret))
            {
                logger.LogWarning("Failed to verify chat-message webhook notification");
                return StatusCode(403);
            }

            var requestType = Request.Headers[TwitchEventSubMessageType];

            if (requestType == TwitchEventSubChallenge)
            {
                logger.LogInformation("Received chat-message webhook challenge");
                return HandleChallenge(notification);
            }

            if (requestType == TwitchEventSubNotification)
            {
                _ = HandleChatNotification(notification);

                return NoContent();
            }

            logger.LogWarning("Received chat-message webhook notification with unknown message type: {type}", requestType);

            return Ok();
        }

        private async Task HandleChatNotification(JsonNode notification)
        {
            var twitchManagerDbContext = await dbContextFactory.CreateDbContextAsync();

            try
            {
                var eventSub = await GetEventSubAsync(notification);
                if(eventSub != null && eventSub.Status == EventSubStatus.Pending)
                {
                    eventSub.Status = EventSubStatus.Enabled;
                    await twitchManagerDbContext.EventSubs
                        .Where(e => e.Id == eventSub.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, eventSub.Status));
                }   

                var message = notification["event"]["message"]["text"].ToString();  

                var userId = GetUserId(notification);

                switch(userId)
                {
                    case "1290701796": // Skypinobot
                        await HandleSkypinobotAsync(notification, message, twitchManagerDbContext);
                        break;

                    case "1003402715": // Ele
                        await HandleEleAsync(notification, message, twitchManagerDbContext);
                        break;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle chat-message webhook notification");
            }
        }
        private async Task HandleEleAsync(JsonNode notification, string message, TwitchManagerDbContext twitchManagerDbContext)
        {
            if (message.StartsWith("ele", StringComparison.InvariantCultureIgnoreCase))
            {
                var broadcasterId = GetBroadcasterId(notification);
                var chatterUsername = GetChatterUsername(notification);

                if (chatterUsername == "ele_fan_tino")
                    return;

                var eleEnabled = await twitchManagerDbContext.LiveDataOptions
                    .Where(e => e.StreamerId == broadcasterId && e.Key == "IsEleEnabled")
                    .FirstOrDefaultAsync();

                if (eleEnabled == null || eleEnabled.Value != "true")
                {
                    await SendMessageInChatAsync(broadcasterId, GetUserId(notification), "Ele non è attiva al momento.");
                    return;
                }
                
                logger.LogInformation("Handling Ele notification with message");

                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://cheshire-cat-core/auth/token");
                var json = new JsonObject
                {
                    ["username"] = "admin",
                    ["password"] = "X6apPZ2ja8uUdciO",
                };
                var content = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
                request.Content = content;
                var tokenResponseMessage = await httpClient.SendAsync(request);

                if (!tokenResponseMessage.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to get token from cheshire-cat-core, status code: {statusCode}", tokenResponseMessage.StatusCode);
                    return;
                }

                var tokenResponse = await tokenResponseMessage.Content.ReadAsStringAsync();

                var tokenJson = JsonNode.Parse(tokenResponse);

                var token = tokenJson?["access_token"]?.ToString();

                var client = new ClientWebSocket();
                await client.ConnectAsync(new Uri($"ws://cheshire-cat-core/ws?token={token}"), CancellationToken.None);

                json = new JsonObject
                {
                    ["text"] = message,
                };

                await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json.ToJsonString())), WebSocketMessageType.Text, true, CancellationToken.None);

                var buffer = new ArraySegment<byte>(new byte[1024]);

                var responseMessage = string.Empty;
                var currentJson = string.Empty;

                while (client.State == WebSocketState.Open)
                {
                    var result = await client.ReceiveAsync(buffer, CancellationToken.None);

                    var resultString = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                    try
                    {
                        currentJson += resultString;

                        var jsonNode = JsonNode.Parse(currentJson);

                        if (jsonNode?["type"]?.ToString() == "chat_token")
                        {
                            responseMessage += jsonNode?["content"]?.ToString();
                        }

                        currentJson = string.Empty;
                    }
                    catch
                    {
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Invalid JSON", CancellationToken.None);
                        break;
                    }
                }

                await SendMessageInChatAsync(GetBroadcasterId(notification), GetUserId(notification), responseMessage);

                logger.LogInformation("Ele notification handled successfully");
            }
            else
            {
                var regex = CommandRegex();

                var match = regex.Match(message);

                if (match.Success)
                {
                    var command = match.Groups[1].Value.Replace("!", "");
                    var option = match.Groups[2].Value;

                    switch (command)
                    {
                        case "attivaEle":
                            await HandleEnableEleAsync(notification, twitchManagerDbContext);
                            break;

                        case "disattivaEle":
                            await HandleDisableEleAsync(notification, twitchManagerDbContext);
                            break;
                    }
                }
            }
        }

        private async Task HandleSkypinobotAsync(JsonNode notification, string message, TwitchManagerDbContext twitchManagerDbContext)
        {
            var regex = CommandRegex();

            var match = regex.Match(message);

            if (match.Success)
            {
                var command = match.Groups[1].Value.Replace("!", "");
                var option = match.Groups[2].Value;

                switch (command)
                {
                    case "entra":
                        await HandleAddOnQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "esci":
                        await HandleRemoveFromQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "lista":
                        await HandleListQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "prossimi":
                        await HandleNextFromQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "svuota":
                        await HandleClearQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "apri":
                        await HandleOpenQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "chiudi":
                        await HandleCloseQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "aiuto":
                        await HandleHelpQueueAsync(notification, twitchManagerDbContext);
                        break;
                    case "vota":
                    case "voto":
                        if (string.IsNullOrEmpty(option))
                        {
                            await SendMessageInChatAsync(GetBroadcasterId(notification), GetUserId(notification), "Devi specificare un gioco da votare. Esempio: !vota NomeGioco oppure !voto NomeGioco");
                            return;
                        }
                        await HandleVoteAsync(notification, option, twitchManagerDbContext);
                        break;
                    case "skyClip":
                    case "SkyClip":
                    case "skyclip":
                    case "clip":
                        await HandleCreateClipAsync(notification);
                        break;
                    default:
                        logger.LogWarning("Unknown command received: {command}", command);
                        break;
                }
            }
        }

        private async Task HandleVoteAsync(JsonNode notification, string vote, TwitchManagerDbContext twitchManagerDbContext)
        {
            var broadcasterId = GetBroadcasterId(notification);
            var userId = GetUserId(notification);
            var chatterUsername = GetChatterUsername(notification);
            var chatterUserId = GetUserId(notification);

            var isSubscriber = IsSubscribed(notification);

            var responseMessage = "";

            if(isSubscriber)
            {
                try
                {
                    var streamerVotedGame = await twitchManagerDbContext.StreamerVotedGames
                .Where(s => s.StreamerId == broadcasterId && s.ChatterUsername == chatterUsername)
                .FirstOrDefaultAsync();

                    if (streamerVotedGame == null)
                    {
                        streamerVotedGame = new StreamerVotedGame
                        {
                            Id = Guid.NewGuid().ToString(),
                            StreamerId = broadcasterId,
                            ChatterId = chatterUserId,
                            ChatterUsername = chatterUsername,
                        };
                        twitchManagerDbContext.StreamerVotedGames.Add(streamerVotedGame);
                    }

                    streamerVotedGame.GameName = vote;
                    streamerVotedGame.VotedOn = DateTime.UtcNow;

                    await twitchManagerDbContext.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Failed to handle vote");
                    return;
                }

                responseMessage = $"@{chatterUsername} Voto ricevuto: {vote}";
            }
            else
            {
                responseMessage = $"@{chatterUsername} Devi essere abbonato per votare";
            }

            await SendMessageInChatAsync(broadcasterId, userId, responseMessage);
        }

        private async Task SendMessageInChatAsync(string broadcasterId, string userId, string message)
        {

            var client = httpClientFactory.CreateClient("TwitchApi");

            var request = new SendMessageRequestMessage(broadcasterId, userId, message);

            var response = await client.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Ok wih content: {content}", content);
            }
            else
            {
                logger.LogWarning("Failed with content: {content}", content);
            }
        }

        private static bool IsSubscribed(JsonNode notification)
        {
            return notification["event"]["badges"].AsArray().Any(b => b["set_id"].ToString() == "subscriber");
        }

        private static bool IsModerator(JsonNode notification)
        {
            return notification["event"]["badges"].AsArray().Any(b => b["set_id"].ToString() == "moderator");
        }

        private static string GetBroadcasterId(JsonNode notification)
        {
            return notification["event"]["broadcaster_user_id"].ToString();
        }

        private static string GetBroadcasterUsername(JsonNode notification)
        {
            return notification["event"]["broadcaster_user_name"].ToString();
        }

        private static string GetUserId(JsonNode notification)
        {
            return notification["subscription"]["condition"]["user_id"].ToString();
        }

        private static string GetChatterId(JsonNode notification)
        {
            return notification["event"]["chatter_user_id"].ToString();
        }

        private static string GetChatterUsername(JsonNode notification)
        {
            return notification["event"]["chatter_user_name"].ToString();
        }

        private static bool IsBroadcaster(JsonNode notification)
        {
            return GetBroadcasterUsername(notification) == GetChatterUsername(notification);
        }

        private async Task HandleEnableEleAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            var isBroadcaster = IsBroadcaster(notification);
            var isModerator = IsModerator(notification);
            var broadcasterId = GetBroadcasterId(notification);
            var userId = GetUserId(notification);
            var chatterUsername = GetChatterUsername(notification);

            if (!isBroadcaster && !isModerator)
            {
                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} ti piacerebbe! Kappa");
                return;
            }

            var eleEnabled = await twitchManagerDbContext.LiveDataOptions
                .Where(e => e.StreamerId == broadcasterId && e.Key == "IsEleEnabled")
                .FirstOrDefaultAsync();

            if (eleEnabled == null)
            {
                eleEnabled = new LiveDataOption
                {
                    Id = Guid.NewGuid().ToString(),
                    StreamerId = broadcasterId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    Key = "IsEleEnabled",
                    Value = "true"
                };
                twitchManagerDbContext.LiveDataOptions.Add(eleEnabled);
            }
            else
            {
                eleEnabled.Value = "true";
                eleEnabled.UpdatedOn = DateTime.UtcNow;
            }

            await twitchManagerDbContext.SaveChangesAsync();

            await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Ele è stata attivata!");
        }

        private async Task HandleDisableEleAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            var isBroadcaster = IsBroadcaster(notification);
            var isModerator = IsModerator(notification);
            var broadcasterId = GetBroadcasterId(notification);
            var userId = GetUserId(notification);
            var chatterUsername = GetChatterUsername(notification);

            if (!isBroadcaster && !isModerator)
            {
                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} ti piacerebbe! Kappa");
                return;
            }

            var eleEnabled = await twitchManagerDbContext.LiveDataOptions
                .Where(e => e.StreamerId == broadcasterId && e.Key == "IsEleEnabled")
                .FirstOrDefaultAsync();

            if (eleEnabled == null)
            {
                eleEnabled = new LiveDataOption
                {
                    Id = Guid.NewGuid().ToString(),
                    StreamerId = broadcasterId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    Key = "IsEleEnabled",
                    Value = "false"
                };
            }
            else
            {
                eleEnabled.Value = "false";
                eleEnabled.UpdatedOn = DateTime.UtcNow;
            }

            await twitchManagerDbContext.SaveChangesAsync();

            await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Ele è stata disattivata!");
        }

        private async Task HandleAddOnQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var isBroadcaster = IsBroadcaster(notification);
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);

                if (isBroadcaster)
                {
                    return; // The broadcaster can't add themselves to the queue
                }

                var isSubscriber = IsSubscribed(notification);

                if (!isSubscriber)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Devi essere abbonato per aggiungerti alla coda");

                    return;
                }

                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null || !gameQueue.IsOpen)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda non è aperta al momento");
                    return;
                }

                var existingUser = await twitchManagerDbContext.GameQueueUsers
                    .Where(u => u.Username == userId && u.GameQueue.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Sei già in coda");
                    return;
                }

                var gameQueueUser = new GameQueueUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = chatterUsername,
                    GameQueueId = gameQueue.Id,
                    AddedOn = DateTime.UtcNow
                };

                await twitchManagerDbContext.GameQueueUsers.AddAsync(gameQueueUser);

                await twitchManagerDbContext.SaveChangesAsync();

                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Sei stato aggiunto in coda");
            }
            catch
            (Exception ex)
            {
                logger.LogError(ex, "Failed to handle add-on-queue notification");
            }
        }

        private async Task HandleRemoveFromQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var isBroadcaster = IsBroadcaster(notification);
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);

                if (isBroadcaster)
                {
                    return; // The broadcaster can't remove themselves from the queue
                }

                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null || !gameQueue.IsOpen)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda non è aperta al momento");
                    return;
                }

                var existingUser = await twitchManagerDbContext.GameQueueUsers
                    .Where(u => u.Username == userId && u.GameQueue.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (existingUser == null)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Non sei in coda");
                    return;
                }

                twitchManagerDbContext.GameQueueUsers.Remove(existingUser);
                await twitchManagerDbContext.SaveChangesAsync();

                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Sei stato rimosso dalla coda");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle remove-from-queue notification");
            }
        }

        private async Task HandleListQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);

                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null || !gameQueue.IsOpen)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda non è aperta al momento");
                    return;
                }
                var queueUsers = await twitchManagerDbContext.GameQueueUsers
                    .Where(u => u.GameQueue.StreamerId == broadcasterId)
                    .OrderBy(u => u.AddedOn)
                    .Select(u => u.Username)
                    .ToListAsync();

                if (queueUsers.Count == 0)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda è vuota");
                }
                else
                {
                    var queueList = string.Join(", ", queueUsers);
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Utenti in coda: {queueList}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle list-queue notification");
            }
        }

        private async Task HandleNextFromQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);

                var isBroadcater = IsBroadcaster(notification);
                var isModerator = IsModerator(notification);

                if(!isBroadcater && !isModerator)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} ti piacerebbe! Kappa");
                    return;
                }

                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null || !gameQueue.IsOpen)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda non è aperta al momento");
                    return;
                }

                var nextUsers = await twitchManagerDbContext.GameQueueUsers
                    .Where(u => u.GameQueue.StreamerId == broadcasterId)
                    .OrderBy(u => u.AddedOn)
                    .Skip(0)
                    .Take(2)
                    .ToListAsync();

                if (nextUsers.Count == 0)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda è vuota");
                    return;
                }

                twitchManagerDbContext.GameQueueUsers.RemoveRange(nextUsers);
                await twitchManagerDbContext.SaveChangesAsync();

                var queueList = string.Join(", ", nextUsers.Select(n => $"@{n.Username}"));

                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} I Prossimi utenti sono: {queueList}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle next-from-queue notification");
            }
        }

        private async Task HandleClearQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);
                var isBroadcater = IsBroadcaster(notification);
                var isModerator = IsModerator(notification);

                if (!isBroadcater && !isModerator)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} ti piacerebbe! Kappa");
                    return;
                }

                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null || !gameQueue.IsOpen)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda non è aperta al momento");
                    return;
                }

                await twitchManagerDbContext.GameQueueUsers
                    .Where(u => u.GameQueue.StreamerId == broadcasterId)
                    .ExecuteDeleteAsync();

                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda è stata svuotata");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle clear-queue notification");
            }
        }

        private async Task HandleOpenQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);
                var isBroadcater = IsBroadcaster(notification);
                var isModerator = IsModerator(notification);

                if (!isBroadcater && !isModerator)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} ti piacerebbe! Kappa");
                    return;
                }

                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null)
                {
                    gameQueue = new GameQueue
                    {
                        Id = Guid.NewGuid().ToString(),
                        StreamerId = broadcasterId,
                        IsOpen = true,
                    };
                    await twitchManagerDbContext.GameQueues.AddAsync(gameQueue);
                }
                else
                {
                    gameQueue.IsOpen = true;
                    twitchManagerDbContext.GameQueues.Update(gameQueue);
                }
                await twitchManagerDbContext.SaveChangesAsync();
                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda è stata aperta");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle open-queue notification");
            }
        }

        private async Task HandleCloseQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);
                var isBroadcater = IsBroadcaster(notification);
                var isModerator = IsModerator(notification);
                if (!isBroadcater && !isModerator)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} ti piacerebbe! Kappa");
                    return;
                }
                var gameQueue = await twitchManagerDbContext.GameQueues
                    .Where(g => g.StreamerId == broadcasterId)
                    .FirstOrDefaultAsync();

                if (gameQueue == null || !gameQueue.IsOpen)
                {
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda è chiusa");
                    return;
                }

                gameQueue.IsOpen = false;
                twitchManagerDbContext.GameQueues.Update(gameQueue);
                
                await twitchManagerDbContext.SaveChangesAsync();
                
                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} La coda è stata chiusa");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle close-queue notification");
            }
        }

        private async Task HandleHelpQueueAsync(JsonNode notification, TwitchManagerDbContext twitchManagerDbContext)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);
                var helpMessage = "Comandi disponibili:\n" +
                                  "!entra - Entra in coda (Devi essere abbonato)\n" +
                                  "!esci - Esci dalla coda\n";

                await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} {helpMessage}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle help-queue notification");
            }
        }

        private async Task HandleCreateClipAsync(JsonNode notification)
        {
            try
            {
                var broadcasterId = GetBroadcasterId(notification);
                var userId = GetUserId(notification);
                var chatterUsername = GetChatterUsername(notification);

                var isModerator = IsModerator(notification);
                var isBroadcater = IsBroadcaster(notification);

                //if (!isBroadcater && !isModerator)
                //{
                //    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} funzione al momento disponibile solo ai mod.");
                //    return;
                //}

                var twitchHttpClient = httpClientFactory.CreateClient("TwitchApi");

                try
                {
                    var request = new CreateClipHttpRequestMessage(broadcasterId, userId);
                    var clipCreationResponse = await twitchHttpClient.SendAsync(request);

                    var message = "";

                    if (!clipCreationResponse.IsSuccessStatusCode)
                    {
                        message += "Errore nella creazione del clip. Riprova più tardi.";
                        logger.LogError("Failed to create clip for broadcaster {broadcasterId}: {statusCode}", broadcasterId, clipCreationResponse.StatusCode);
                    }
                    else
                    {
                        var model = await request.GetDataAsync(clipCreationResponse);

                        var url = $"https://www.twitch.tv/{GetBroadcasterUsername(notification)}/clip/{model.Data.First().Id}";

                        message += $"Clip creata con successo: {url}";
                    }

                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} {message}");
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Failed to create clip for broadcaster {broadcasterId}", broadcasterId);
                    await SendMessageInChatAsync(broadcasterId, userId, $"@{chatterUsername} Si è verificato un errore generico. Riprova più tardi");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle help-queue notification");
            }
        }

        [GeneratedRegex(@"^(!\w+)(?:\s+(.*))?$")]
        private static partial Regex CommandRegex();
    }
}
