using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Streamers;

namespace TwitchManager.Services.Abstractions
{
    public interface IStreamerService
    {
        Task<IEnumerable<StreamerModel>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<StreamerModel> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        Task AddAsync(StreamerModel streamer, CancellationToken cancellationToken = default);

        Task AddAsync(StreamerDataModel streamer, CancellationToken cancellationToken = default);

        Task<StreamerDataModel> GetStreamerFromTwitchAsync(string username, CancellationToken cancellationToken = default);
    }
}
