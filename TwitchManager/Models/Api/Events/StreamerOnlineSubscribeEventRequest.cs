using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Events
{
    public class StreamerOnlineSubscribeEventRequest : SubscribeEventRequest<StreamerOnlineSubscribeEventRequest.ConditionModel>
    {
        public class ConditionModel
        {
            [JsonPropertyName("broadcaster_user_id")]
            public string BroadcasterUserId { get; set; }
        }

        public override string GetCondition()
        {
            return JsonSerializer.Serialize(Condition);
        }

        public StreamerOnlineSubscribeEventRequest(string brodcasterUserId, string callback) : base(callback)
        {
            Version = "1";
            Condition = new ConditionModel
            {
                BroadcasterUserId = brodcasterUserId
            };
            Type = TwitchEvents.StreamOnline;
        }
    }
}
