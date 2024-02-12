using TwitchManager.Models.Api.Clips.Data;

namespace TwitchManager.Services.Abstractions
{
    public interface IClipService
    {
        Task<List<ClipDataModel>> GetAllAsync();
    }
}
