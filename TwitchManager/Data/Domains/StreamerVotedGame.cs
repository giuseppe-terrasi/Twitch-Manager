namespace TwitchManager.Data.Domains
{
    public class StreamerVotedGame
    {
        public string Id { get; set; }

        public string StreamerId { get; set; }

        public string ChatterId { get; set; }

        public string ChatterUsername { get; set; }

        public string GameName { get; set; }

        public DateTime VotedOn { get; set; }
    }
}
