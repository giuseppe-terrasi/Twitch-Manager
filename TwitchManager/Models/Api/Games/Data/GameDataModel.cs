using System.Text.Json.Serialization;

namespace TwitchManager.Models.Api.Games.Data
{
    public class GameDataModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("box_art_url")]
        public string BoxArtUrl { get; set; }

        [JsonPropertyName("igdb_id")]
        public string IgdbId { get; set; }
    }
}
