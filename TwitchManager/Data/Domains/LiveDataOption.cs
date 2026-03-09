namespace TwitchManager.Data.Domains
{
    public class LiveDataOption
    {
        public string Id { get; set; }

        public string StreamerId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public virtual Streamer Streamer { get; set; }
    }
}
