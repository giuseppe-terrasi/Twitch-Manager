using System.Text;
using System.Text.Json;

using TwitchManager.Models.Api.Events;

namespace TwitchManager.Comunications.TwicthApi.Api.Events
{
    public class SubscribeEventRequestMessage<T> : TwitchApiHttpRequestMessage
        where T : SubscribeEventRequest
    {
        public SubscribeEventRequestMessage(T model)
        {
            RequestUri = new("eventsub/subscriptions", UriKind.Relative);
            Method = HttpMethod.Post;
            var json = JsonSerializer.Serialize(model);
            Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
