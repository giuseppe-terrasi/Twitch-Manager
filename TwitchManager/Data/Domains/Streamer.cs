namespace TwitchManager.Data.Domains
{
    public class Streamer
    {
        public string Id { get; set; }
        public string Login { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string BroadcasterType { get; set; }
        public string Description { get; set; }
        public string ProfileImageUrl { get; set; }
        public string OfflineImageUrl { get; set; }
        public int ViewCount { get; set; }
        public string CreatedAt { get; set; }

        public virtual ICollection<Clip> Clips { get; set; }
    }
}
