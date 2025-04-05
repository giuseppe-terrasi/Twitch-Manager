using System.Text;
using System.Text.Json.Nodes;

using TwitchManager.Models.Api.Messages.Responses;

namespace TwitchManager.Comunications.TwicthApi.Api.Messages
{
    public class SendMessageRequestMessage : TwitchApiHttpRequestMessage<SendMessageResponseModel>
    {
        public SendMessageRequestMessage(string broadcasterId, string senderId, string message)
        {
            var json = new JsonObject
            {
                ["broadcaster_id"] = broadcasterId,
                ["sender_id"] = senderId,
                ["message"] = message
            };

            Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            RequestUri = new Uri($"chat/messages", UriKind.Relative);
            Method = HttpMethod.Post;
        }
    }
}
