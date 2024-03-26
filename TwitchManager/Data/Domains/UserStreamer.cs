namespace TwitchManager.Data.Domains
{
    public class UserStreamer
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string StreamerId { get; set; }

        public bool IsClipDefault { get; set; }

        public virtual User User { get; set; }

        public virtual Streamer Streamer { get; set; }  
    }
}
