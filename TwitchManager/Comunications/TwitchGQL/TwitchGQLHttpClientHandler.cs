
namespace TwitchManager.Comunications.TwitchGQL
{
    public class TwitchGQLHttpClientHandler : HttpClientHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var task = SendAsync(request, cancellationToken);
            task.Wait(cancellationToken);
            return task.Result;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Client-Id", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
