using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Streamers;

namespace TwitchManager.Services.Abstractions
{
    public interface IStreamerService
    {
        Task<IEnumerable<StreamerModel>> GetAllAsync();

        Task<StreamerModel> GetByIdAsync(string id);

        Task AddAsync(StreamerModel streamer);

        Task AddAsync(StreamerDataModel streamer);

        Task<StreamerDataModel> GetStreamerFromTwitchAsync(string username);
    }
}
