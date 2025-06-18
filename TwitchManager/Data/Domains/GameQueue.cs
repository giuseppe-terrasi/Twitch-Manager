namespace TwitchManager.Data.Domains
{
    public class GameQueue
    {
        public string Id { get; set; }

        public string StreamerId { get; set; }

        public string GameName { get; set; }

        public bool IsOpen { get; set; }

        public virtual ICollection<GameQueueUser> Users { get; set; }
    }

    public class GameQueueUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string GameQueueId { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.UtcNow;    

        public GameQueue GameQueue { get; set; }
    }
}
