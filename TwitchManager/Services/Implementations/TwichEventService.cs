using AutoMapper;

using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

using TwitchManager.Comunications.TwicthApi;
using TwitchManager.Comunications.TwicthApi.Api.Events;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Models.Api.Events;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    internal class TwichEventService(ILogger<TwichEventService> logger, IHttpClientFactory httpClientFactory, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper) : ITwichEventService
    {
        private static string GenerateRandomString()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] tokenData = new byte[16];
            rng.GetBytes(tokenData);

            var token = Convert.ToBase64String(tokenData);

            return token;
        }

        public async Task<ICollection<EventSubModel>> GetSubscriptionsAsync(string streamerId, CancellationToken cancellationToken)
        {
            try
            {
                var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var eventSubs = await dbContext.EventSubs.AsNoTracking()
                    .Where(e => e.StreamerId == streamerId)
                    .ToListAsync(cancellationToken);

                var mapped = mapper.Map<ICollection<EventSubModel>>(eventSubs);

                return mapped;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to list subscriptions");
                throw;
            }
        }

        public async Task<bool> SubscribeAsync<T>(T eventModel, string streamerId, CancellationToken cancellationToken)
            where T : SubscribeEventRequest
        {
            var eventName = eventModel.Type;
            var result = true;

            try
            {
                var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var eventSub = await dbContext.EventSubs
                    .Where(e => e.Type == eventName && e.StreamerId == streamerId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (eventSub is not null && eventSub.Status == EventSubStatus.Enabled)
                {
                    logger.LogInformation("Already subscribed to {eventName} event", eventName);
                    return result;
                }
                else eventSub ??= new EventSub
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = eventName,
                    Version = eventModel.Version,
                    Condition = eventModel.GetCondition(),
                    Callback = eventModel.Transport.Callback,
                    Secret = eventModel.Transport.Secret,
                    Method = eventModel.Transport.Method,
                    StreamerId = streamerId
                };

                eventModel.Transport.Secret = GenerateRandomString();
                eventSub.Secret = eventModel.Transport.Secret;

                TwitchApiHttpRequestMessage request = new SubscribeEventRequestMessage<T>(eventModel);

                var client = httpClientFactory.CreateClient("TwitchApi");

                var response = await client.SendAsync(request, cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var node = JsonSerializer.Deserialize<JsonNode>(content);

                    logger.LogInformation("Subscribed to {eventName} event", eventName);
                    eventSub.Status = EventSubStatus.Pending;
                    eventSub.TwitchEventId = node["data"][0]["id"].ToString();
                    eventSub.RequestError = null;
                }
                else
                {
                    eventSub.Status = EventSubStatus.RequestFailed;
                    eventSub.RequestError = $"StatusCode: {response.StatusCode}, Content: {content}";   

                    logger.LogError("Failed to subscribe to {eventName} event: {response}", eventName, content);
                    result = false;
                }

                if (dbContext.Entry(eventSub).State == EntityState.Detached)
                {
                    await dbContext.EventSubs.AddAsync(eventSub, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to subscribe to {eventName} event", eventName);
                result = false;
            }

            return result;
        }

        public async Task<bool> DeleteSubscriptionAsync(string id, CancellationToken cancellationToken)
        {
            var result = true;
            try
            {
                var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var eventSub = await dbContext.EventSubs.Where(e => e.Id == id).SingleOrDefaultAsync(cancellationToken);

                if (eventSub is null)
                {
                    logger.LogInformation("Subscription {id} not found", id);
                    return result;
                }

                TwitchApiHttpRequestMessage request = new UnsubscribeEventRequestMessage(eventSub.TwitchEventId);

                var client = httpClientFactory.CreateClient("TwitchApi");

                var response = await client.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Unsubscribed from {eventName} event", eventSub.Type);
                    
                    eventSub.Status = EventSubStatus.Disabled;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    eventSub.Status = EventSubStatus.RequestFailed;
                    eventSub.RequestError = $"StatusCode: {response.StatusCode}, Content: {content}";
                    logger.LogError("Failed to unsubscribe from {eventName} event: {response}", eventSub.Type, content);
                    result = false;
                }

                await dbContext.SaveChangesAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete subscription {id}", id);
                result = false;
            }

            return result;
        }
    }
}
