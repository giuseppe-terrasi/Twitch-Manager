using Microsoft.AspNetCore.Http.Extensions;

using TwitchManager.Models.Api.Clips.Responses;


namespace TwitchManager.Comunications.TwicthApi.Api.Clips
{
    public class CreateClipHttpRequestMessage : TwitchApiHttpRequestMessage<ClipCreationDataResponseModel>
    {
        public CreateClipHttpRequestMessage(string broadcasterId, string senderId)
        {

            BotUserId = senderId;
            var query = new QueryBuilder();

            if (!string.IsNullOrEmpty(broadcasterId))
                query.Add("broadcaster_id", broadcasterId);

            RequestUri = new Uri($"clips{query.ToQueryString()}", UriKind.Relative);
            Method = HttpMethod.Post;

        }
    }
}
