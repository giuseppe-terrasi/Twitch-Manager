namespace TwitchManager.Data.Domains
{
    public class DiscordChannel
    {
        public string Id { get; set; }

        public string ChannelId { get; set; }

        public string StreamerId { get; set; }

        public virtual Streamer Streamer { get; set; }
    }
}
