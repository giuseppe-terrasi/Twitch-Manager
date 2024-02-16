using Microsoft.AspNetCore.Http.Extensions;

using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Api.Clips.Responses;

namespace TwitchManager.Comunications.TwicthApi.Api.Clips
{
    public class GetStreamerHttpRequestMessage : TwitchApiHttpRequestMessage<StreamerDataResponseModel>  
    {

        public GetStreamerHttpRequestMessage(string id = null, string login = null)
        {
            var query = new QueryBuilder();

            if (!string.IsNullOrEmpty(id))
                query.Add("id", id);
            if (!string.IsNullOrEmpty(login))
                query.Add("login", login);

            RequestUri = new Uri($"users{query.ToQueryString()}", UriKind.Relative);
            Method = HttpMethod.Get;
        }
    }
}
