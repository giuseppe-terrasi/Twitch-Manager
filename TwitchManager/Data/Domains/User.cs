namespace TwitchManager.Data.Domains
{
    public class User
    {
        public string Id { get; set; }

        public string TwitchId { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual ICollection<ClipVote> ClipVotes { get; set; }
    }
}
