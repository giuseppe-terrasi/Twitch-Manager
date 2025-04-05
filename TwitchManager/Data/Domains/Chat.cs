namespace TwitchManager.Data.Domains
{
    public class Chat
    {
        public Guid Id { get; set; }

        public string StreamerId { get; set; }

        public string RawData { get; set; }

        public virtual Streamer Streamer { get; set; }
    }
}
