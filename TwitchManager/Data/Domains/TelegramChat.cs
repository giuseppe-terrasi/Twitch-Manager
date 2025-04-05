namespace TwitchManager.Data.Domains
{
    public class TelegramChat
    {
        public string Id { get; set; }

        public string ChatId { get; set; }

        public string StreamerId { get; set; }

        public virtual Streamer Streamer { get; set; }  
    }
}
