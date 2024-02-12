using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Abstractions
{
    public abstract class ResponseModel<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
