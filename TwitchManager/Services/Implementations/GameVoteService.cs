using Microsoft.EntityFrameworkCore;

using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Helpers;
using TwitchManager.Models;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class GameVoteService(ILogger<GameVoteService> logger, IDbContextFactory<TwitchManagerDbContext> dbContextFactory,
        IHttpContextAccessor httpContextAccessor) : IGameVoteService
    {
        private IQueryable<StreamerVotedGame> GetQuery(TwitchManagerDbContext dbContext)
        {
            var isAdmin = httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
            var streamerId = httpContextAccessor.HttpContext?.User.GetUserTwitchId();

            var query = dbContext.StreamerVotedGames.AsNoTracking();
            if (!isAdmin && !string.IsNullOrEmpty(streamerId))
            {
                query = query.Where(x => x.StreamerId == streamerId);
            }
            return query;
        }

        public async Task<List<GameVoteModel>> GetVotesAsync()
        {
            try
            {
                var dbContext = dbContextFactory.CreateDbContext();

                return await GetQuery(dbContext)
                    .Select(x => new GameVoteModel
                    {
                        Id = x.Id,
                        GameName = x.GameName,
                        ChatterUsername = x.ChatterUsername,
                        VotedOn = x.VotedOn
                    })
                    .OrderBy(x => x.VotedOn)
                        .ThenBy(x => x.GameName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving game votes");
                return [];
            }
        }

        public async Task<GameVoteModel> GetRandomAsync()
        {
            var dbContext = dbContextFactory.CreateDbContext();
            var query = GetQuery(dbContext);

            var count = await dbContext.StreamerVotedGames.CountAsync();

            if (count == 0)
            {
                return null;
            }

            var randomIndex = Random.Shared.Next(0, count);

            var randomVote = await query
                .OrderBy(x => x.VotedOn)
                .ThenBy(x => x.GameName)
                .Skip(randomIndex)
                .Take(1)
                .Select(x => new GameVoteModel
                {
                    Id = x.Id,
                    GameName = x.GameName,
                    ChatterUsername = x.ChatterUsername,
                    VotedOn = x.VotedOn
                })
                .FirstOrDefaultAsync();

            return randomVote;
        }

        public async Task ClearVotesAsync()
        {
            var dbContext = dbContextFactory.CreateDbContext();
            var query = GetQuery(dbContext);

            await query.ExecuteDeleteAsync();
        }
    }
}
