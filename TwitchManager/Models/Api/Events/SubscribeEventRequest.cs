using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Events
{
    public abstract class SubscribeEventRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("transport")]
        public SubscribeEventRequestTransport Transport { get; set; }

        abstract public string GetCondition();

        public SubscribeEventRequest()
        {
            Transport = new();
        }

        public SubscribeEventRequest(string callback)
        {
            Transport = new SubscribeEventRequestTransport(callback);
        }

    }

    public abstract class SubscribeEventRequest<T> : SubscribeEventRequest where T : class
    {

        [JsonPropertyName("condition")]
        public T Condition { get; set; }

        public SubscribeEventRequest()
        {
            
        }

        public SubscribeEventRequest(string callback) : base(callback)
        {
            
        }
    }

    public class SubscribeEventRequestTransport
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = "webhook";

        [JsonPropertyName("callback")]
        public string Callback { get; set; }

        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        public SubscribeEventRequestTransport()
        {
            
        }

        public SubscribeEventRequestTransport(string callback)
        {
            Callback = callback;
        }
    }
}
