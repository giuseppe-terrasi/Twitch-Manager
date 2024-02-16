using Microsoft.AspNetCore.Http.Extensions;

using TwitchManager.Models.Api.Clips.Responses;

namespace TwitchManager.Comunications.TwicthApi.Api.Clips
{
    public class GetClipsHttpRequestMessage : TwitchApiHttpRequestMessage<ClipDataResponseModel>
    {

        public GetClipsHttpRequestMessage(string broadcasterId = null, string gameId = null, string startedAt = null, string endedAt = null, string first = null, string after = null, string before = null)
        {
            var query = new QueryBuilder();

            if (!string.IsNullOrEmpty(broadcasterId))
                query.Add("broadcaster_id", broadcasterId);
            if (!string.IsNullOrEmpty(gameId))
                query.Add("game_id", gameId);
            if (!string.IsNullOrEmpty(startedAt))
                query.Add("started_at", startedAt);
            if (!string.IsNullOrEmpty(endedAt))
                query.Add("ended_at", endedAt);
            if (!string.IsNullOrEmpty(first))
                query.Add("first", first);
            if (!string.IsNullOrEmpty(after))
                query.Add("after", after);
            if (!string.IsNullOrEmpty(before))
                query.Add("before", before);

            RequestUri = new Uri($"clips{query.ToQueryString()}", UriKind.Relative);
            Method = HttpMethod.Get;
        }
    }
}
