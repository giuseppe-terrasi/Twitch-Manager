using System.Text;
using System.Text.Json;
using TwitchManager.Comunications.TwicthApi;

namespace TwitchManager.Comunications.TwitchGQL.Clips
{
    public class ClipTokenTwitchGQLRequest : TwitchApiHttpRequestMessage<List<ClipTokenData>>
    {
        public TwitchGQLOperation Operation { get; set; }

        public ClipTokenTwitchGQLRequest(string clipId)
        {
            Operation = new TwitchGQLOperation()
            {
                OperationName = "VideoAccessToken_Clip",
                Variables = new Variables()
                {
                    Slug = clipId
                }
            };

            RequestUri = new Uri("", UriKind.Relative);
            Method = HttpMethod.Post;
            var json = JsonSerializer.Serialize(new[] { Operation });
            Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
