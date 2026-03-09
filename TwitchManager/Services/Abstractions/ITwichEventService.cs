using TwitchManager.Data.Domains;
using TwitchManager.Models.Api.Events;

namespace TwitchManager.Services.Abstractions
{

    public interface ITwichEventService
    {
        Task<ICollection<EventSubModel>> GetSubscriptionsAsync(string streamerId, CancellationToken cancellationToken);

        Task<bool> SubscribeAsync<T>(T eventModel, string streamerId, string botUserId, CancellationToken cancellationToken) where T : SubscribeEventRequest;

        Task<bool> DeleteSubscriptionAsync(string id, CancellationToken cancellationToken);

        Task<List<BotUser>> GetBotUserIdsAsnc(CancellationToken cancellationToken);
    }
}
