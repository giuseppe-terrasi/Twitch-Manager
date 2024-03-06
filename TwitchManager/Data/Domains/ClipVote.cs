namespace TwitchManager.Data.Domains
{
    public class ClipVote
    {
        public string Id { get; set; }

        public string ClipId { get; set; }

        public string UserId { get; set; }

        public virtual Clip Clip { get; set; }

        public virtual User User { get; set; }
    }
}
