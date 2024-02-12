using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Abstractions
{
    public class PaginationDataModel
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}
