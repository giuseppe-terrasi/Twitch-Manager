namespace TwitchManager.Models
{
    public class GameVoteModel
    {
        public string Id { get; set; }

        public string GameName { get; set; }

        public string ChatterUsername { get; set; }

        public DateTime VotedOn { get; set; }
    }
}
