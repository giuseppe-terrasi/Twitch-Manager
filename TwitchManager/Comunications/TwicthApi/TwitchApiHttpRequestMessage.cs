using System.Text.Json;

namespace TwitchManager.Comunications.TwicthApi
{
    public abstract class TwitchApiHttpRequestMessage : HttpRequestMessage
    {
        public bool AuthorizationRequired { get; set; } = true;
    }

    public class TwitchApiHttpRequestMessage<T> : TwitchApiHttpRequestMessage
    {
        public async Task<T> GetDataAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonSerializer.Deserialize<T>(content);
            }

            throw new Exception($"Error: {response.StatusCode}");
        }
    }
}
