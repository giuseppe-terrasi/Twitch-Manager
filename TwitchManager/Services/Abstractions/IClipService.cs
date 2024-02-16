using TwitchManager.Models.Clips;

namespace TwitchManager.Services.Abstractions
{
    public interface IClipService
    {
        Task<IEnumerable<ClipModel>> GetAllAsync();

        Task<IEnumerable<ClipModel>> GetByStreamerAsync(string streamerId);

        Task<string> GetDownloadLinkAsync(string clipUrl, CancellationToken cancellationToken);

        Task<ClipModel> GetByIdAsync(string id);

        Task GetFromApiAsync(string streamerId);

        void CreateChromeDriver();

        void DisposeChromeDriver();
    }
}
