namespace TwitchManager.Comunications.TwicthApi.Api.Events
{
    public class UnsubscribeEventRequestMessage : TwitchApiHttpRequestMessage
    {
        public UnsubscribeEventRequestMessage(string twitchEventId)
        {
            RequestUri = new($"eventsub/subscriptions?id={twitchEventId}", UriKind.Relative);
            Method = HttpMethod.Delete;
        }
    }
}
