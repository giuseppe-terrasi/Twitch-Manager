namespace TwitchManager.Models.Clips
{
    public class ClipModel
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string EmbedUrl { get; set; }
        public string BroadcasterId { get; set; }
        public string BroadcasterName { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string VideoId { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ThumbnailUrl { get; set; }
        public double Duration { get; set; }
        public int? VodOffset { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsUserVoted { get; set; }

        public string DownloadLink { get; set; }

        public int Votes { get; set; }

        public string VideoUrl { get; set; }

        public void Vote()
        {
            IsUserVoted = !IsUserVoted;
            Votes += IsUserVoted ? 1 : -1;
        }
    }
}
