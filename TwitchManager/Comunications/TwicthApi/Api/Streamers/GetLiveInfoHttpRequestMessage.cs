using Microsoft.AspNetCore.Http.Extensions;
using TwitchManager.Models.Api.Clips.Responses;

namespace TwitchManager.Comunications.TwicthApi.Api.Streamers
{
    public class GetLiveInfoHttpRequestMessage : TwitchApiHttpRequestMessage<LiveDataResponseModel>
    {
        public GetLiveInfoHttpRequestMessage(string streamerId)
        {
            var query = new QueryBuilder();

            if (!string.IsNullOrEmpty(streamerId))
                query.Add("user_id", streamerId);

            RequestUri = new Uri($"streams{query.ToQueryString()}", UriKind.Relative);
            Method = HttpMethod.Get;

        }
    }
}
