using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Messages.Data
{
    public class SendMessageResponseData
    {
        [JsonPropertyName("message_id")]
        public string MessageId { get; set; }

        [JsonPropertyName("is_sent")]
        public bool IsSent { get; set; }

        [JsonPropertyName("drop_reason")]
        public string DropReason { get; set; }
    }
}
