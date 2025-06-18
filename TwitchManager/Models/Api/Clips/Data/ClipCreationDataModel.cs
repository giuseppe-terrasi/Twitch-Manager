using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Clips.Data
{
    public class ClipCreationDataModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("edit_url")]
        public string EditUrl { get; set; }
    }
}
