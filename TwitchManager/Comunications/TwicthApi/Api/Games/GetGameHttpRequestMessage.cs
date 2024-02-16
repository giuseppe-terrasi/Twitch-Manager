using Microsoft.AspNetCore.Http.Extensions;
using TwitchManager.Models.Api.Games.Responses;

namespace TwitchManager.Comunications.TwicthApi.Api.Games
{
    public class GetGameHttpRequestMessage : TwitchApiHttpRequestMessage<GameDataResponseModel>
    {

        public GetGameHttpRequestMessage(string id = null)
        {
            var query = new QueryBuilder();

            if (!string.IsNullOrEmpty(id))
                query.Add("id", id);

            RequestUri = new Uri($"games{query.ToQueryString()}", UriKind.Relative);
            Method = HttpMethod.Get;
        }
    }
}
