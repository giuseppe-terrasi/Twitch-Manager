using AutoMapper;
using Microsoft.EntityFrameworkCore;

using System.Net.Http;

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
        public async Task<IEnumerable<StreamerModel>> GetAllAsync()
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var streamers = await context.Streamers
                .Select(c => mapper.Map<StreamerModel>(c))
                .ToListAsync();

            return streamers;
        }

        public async Task<StreamerModel> GetByIdAsync(string id)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var streamer = await context.Streamers
                .Where(c => c.Id == id)
                .Select(c => mapper.Map<StreamerModel>(c))
                .FirstOrDefaultAsync();

            return streamer;
        }

        public async Task AddAsync(StreamerModel streamer)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            context.Streamers.Add(mapper.Map<Streamer>(streamer));

            await context.SaveChangesAsync();
        }

        public async Task AddAsync(StreamerDataModel streamer)
        {
            await AddAsync(mapper.Map<StreamerModel>(streamer));
        }

        public async Task<StreamerDataModel> GetStreamerFromTwitchAsync(string username)
        {
            var client = httpClientFactory.CreateClient("TwitchApi");

            var request = new GetStreamerHttpRequestMessage(login: username);

            var response = await client.SendAsync(request);

            var streamers = await request.GetDataAsync(response);

            return streamers.Data.FirstOrDefault();
        }
    }
}
