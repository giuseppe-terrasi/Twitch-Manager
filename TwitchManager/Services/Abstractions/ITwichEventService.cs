using TwitchManager.Models.Api.Events;

namespace TwitchManager.Services.Abstractions
{

    public interface ITwichEventService
    {
        Task<ICollection<EventSubModel>> GetSubscriptionsAsync(string streamerId, CancellationToken cancellationToken);

        Task<bool> SubscribeAsync<T>(T eventModel, string streamerId, CancellationToken cancellationToken) where T : SubscribeEventRequest;

        Task<bool> DeleteSubscriptionAsync(string id, CancellationToken cancellationToken);
    }
}
