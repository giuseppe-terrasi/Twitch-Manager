using AutoMapper;
using Microsoft.EntityFrameworkCore;

using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Streamers;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class StreamerService(IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper, IHttpClientFactory httpClientFactory) : IStreamerService
    {
        public async Task<IEnumerable<StreamerModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var streamers = await context.Streamers
                .Select(c => mapper.Map<StreamerModel>(c))
                .ToListAsync(cancellationToken);

            return streamers;
        }

        public async Task<StreamerModel> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var streamer = await context.Streamers
                .Where(c => c.Id == id)
                .Select(c => mapper.Map<StreamerModel>(c))
                .FirstOrDefaultAsync(cancellationToken);

            return streamer;
        }

        public async Task AddAsync(StreamerModel streamer, CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            context.Streamers.Add(mapper.Map<Streamer>(streamer));

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddAsync(StreamerDataModel streamer, CancellationToken cancellationToken = default)
        {
            await AddAsync(mapper.Map<StreamerModel>(streamer), cancellationToken);
        }

        public async Task<StreamerDataModel> GetStreamerFromTwitchAsync(string username, CancellationToken cancellationToken = default)
        {
            var client = httpClientFactory.CreateClient("TwitchApi");

            var request = new GetStreamerHttpRequestMessage(login: username);

            var response = await client.SendAsync(request, cancellationToken);

            var streamers = await request.GetDataAsync(response, cancellationToken);

            return streamers.Data.FirstOrDefault();
        }
    }
}
