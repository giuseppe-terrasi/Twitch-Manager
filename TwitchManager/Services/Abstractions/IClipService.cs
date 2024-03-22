using System.Linq.Expressions;

using TwitchManager.Models.Api.Games.Data;
using TwitchManager.Models.Clips;
using TwitchManager.Models.Streamers;

namespace TwitchManager.Services.Abstractions
{
    public interface IClipService
    {
        Task<ICollection<ClipModel>> GetAllAsync();

        Task<int> GetTotalByStremaerAsync(string streamerId);

        Task<ICollection<string>> GetCreatorsByStremaerAsync(string streamerId);

        Task<ICollection<string>> GetGamesByStremaerAsync(string streamerId);

        Task<ClipFilterResultModel> GetByStreamerAsync(ClipFilterModel filterModel, CancellationToken cancellationToken = default);

        Task<string> GetDownloadLinkAsync(string clipUrl, CancellationToken cancellationToken);

        Task<ClipModel> GetByIdAsync(string id);

        Task GetFromApiAsync(string streamerId, CancellationToken cancellationToken = default);

        Task VoteAsync(string clipId, bool isUpvote);
    }
}
