using System.Text.Json.Serialization;
using System.Text.Json;

namespace TwitchManager.Models.Api.Events
{
    public class ChannelChatMessageSubscribeEventRequest : SubscribeEventRequest<ChannelChatMessageSubscribeEventRequest.ConditionModel>
    {
        public class ConditionModel
        {
            [JsonPropertyName("broadcaster_user_id")]
            public string BroadcasterUserId { get; set; }

            [JsonPropertyName("user_id")]
            public string UserId { get; set; }

        }

        override public string GetCondition()
        {
            return JsonSerializer.Serialize(Condition);
        }

        public ChannelChatMessageSubscribeEventRequest(string brodcasterUserId, string userId, string callback) : base(callback)
        {
            Version = "1";
            Condition = new ConditionModel
            {
                BroadcasterUserId = brodcasterUserId,
                UserId = userId
            };
            Type = TwitchEvents.ChannelChatMessage;
        }
    }
}
