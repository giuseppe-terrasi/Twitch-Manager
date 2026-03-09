using TwitchManager.Models;

namespace TwitchManager.Services.Abstractions
{
    public interface IGameVoteService
    {
        public Task<List<GameVoteModel>> GetVotesAsync();

        public Task ClearVotesAsync();

        public Task<GameVoteModel> GetRandomAsync();
    }
}
