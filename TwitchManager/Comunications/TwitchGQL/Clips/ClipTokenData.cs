using System.Text.Json.Serialization;
using System.Web;

namespace TwitchManager.Comunications.TwitchGQL.Clips
{
    public class ClipTokenData
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        public string GetUrl()
        {
            if (Data.Clip.VideoQualities.Count == 0) return string.Empty;

            var valueUrlEncode = HttpUtility.UrlEncode(Data.Clip.PlaybackAccessToken.Value);
            return $"{Data.Clip.VideoQualities[0].SourceURL}?sig={Data.Clip.PlaybackAccessToken.Signature}&token={valueUrlEncode}";
        }
    }
    public class Data
    {
        [JsonPropertyName("clip")]
        public ClipInfo Clip { get; set; }
    }

    public class ClipInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("playbackAccessToken")]
        public PlaybackAccessToken PlaybackAccessToken { get; set; }

        [JsonPropertyName("videoQualities")]
        public List<ClipVideoQuality> VideoQualities { get; set; }

        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }
    }


    public class PlaybackAccessToken
    {
        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }
    }

    public class ClipVideoQuality
    {
        [JsonPropertyName("frameRate")]
        public double FrameRate { get; set; }

        [JsonPropertyName("quality")]
        public string Quality { get; set; }

        [JsonPropertyName("sourceURL")]
        public string SourceURL { get; set; }

        [JsonPropertyName("typeName")]
        public string TypeName { get; set; }
    }

}
