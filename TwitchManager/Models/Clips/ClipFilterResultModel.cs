namespace TwitchManager.Models.Clips
{
    public class ClipFilterResultModel
    {
        public ICollection<ClipModel> Clips { get; set; } = [];

        public int FilteredTotal { get; set; }
    }
}
