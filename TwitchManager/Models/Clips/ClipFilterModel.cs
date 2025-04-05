using System.Linq.Expressions;

namespace TwitchManager.Models.Clips
{
    public class ClipFilterModel
    {
        public string StreamerId { get; set; }
        
        public int Skip { get; set; }
        
        public int Take { get; set; }

        public ICollection<Expression<Func<ClipModel, bool>>> Filters { get; set; }

        public string OrderBy { get; set; }

        public bool IsRandom { get; set; }
    }
}
