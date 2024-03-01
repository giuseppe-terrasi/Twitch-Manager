using System.Text.Json.Serialization;

namespace TwitchManager.Comunications.TwitchGQL
{
    public class TwitchGQLOperation()
    {
        [JsonPropertyName("operationName")]
        public string OperationName { get; set; }

        [JsonPropertyName("variables")] 
        public Variables Variables { get; set; } = new Variables();

        [JsonPropertyName("extensions")]
        public Extensions Extensions { get; set; } = new Extensions();
    }

    public class Variables()
    {
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
    }

    public class Extensions
    {
        [JsonPropertyName("persistedQuery")]    
        public PersistedQuery PersistedQuery { get; set; } = new PersistedQuery();
    }

    public class PersistedQuery
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("sha256Hash")]    
        public string Sha256Hash { get; set; } = "36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11";
    }
}
