using TwitchManager.Models.Clips;

namespace TwitchManager.Services.Abstractions
{
    public interface IClipService
    {
        Task<List<ClipModel>> GetAllAsync();

        Task<string> GetDownloadLinkAsync(string clipUrl);

        Task<ClipModel> GetByIdAsync(string id);
    }
}
