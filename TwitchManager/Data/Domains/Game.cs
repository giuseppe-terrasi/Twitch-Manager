namespace TwitchManager.Data.Domains
{
    public class Game
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string BoxArtUrl { get; set; }

        public string IgdbId { get; set; }

        public virtual ICollection<Clip> Clips { get; set; }
    }
}
