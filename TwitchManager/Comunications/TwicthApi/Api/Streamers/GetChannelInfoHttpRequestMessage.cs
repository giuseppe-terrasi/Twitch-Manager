using Microsoft.AspNetCore.Http.Extensions;
using TwitchManager.Models.Api.Clips.Responses;

namespace TwitchManager.Comunications.TwicthApi.Api.Streamers
{
    public class GetChannelInfoHttpRequestMessage : TwitchApiHttpRequestMessage<ChannelDataResponseModel>
    {
        public GetChannelInfoHttpRequestMessage(string streamerId)
        {
            var query = new QueryBuilder();

            if (!string.IsNullOrEmpty(streamerId))
                query.Add("broadcaster_id", streamerId);

            RequestUri = new Uri($"channels{query.ToQueryString()}", UriKind.Relative);
            Method = HttpMethod.Get;

        }
    }
}
