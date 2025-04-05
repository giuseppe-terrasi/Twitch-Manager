using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Events
{
    public class ChannelFollowSubscribeEventRequest : SubscribeEventRequest<ChannelFollowSubscribeEventRequest.ConditionModel>
    {
        public class ConditionModel
        {
            [JsonPropertyName("broadcaster_user_id")]
            public string BroadcasterUserId { get; set; }

            [JsonPropertyName("moderator_user_id")]
            public string ModeratorUserId { get; set; }

        }

        override public string GetCondition()
        {
            return JsonSerializer.Serialize(Condition);
        }

        public ChannelFollowSubscribeEventRequest(string brodcasterUserId, string moderatorUserId, string callback) : base(callback)
        {
            Version = "2";
            Condition = new ConditionModel
            {
                BroadcasterUserId = brodcasterUserId,
                ModeratorUserId = moderatorUserId
            };
            Type = TwitchEvents.ChannelFollow;
        }
    }
}
