using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Helpers;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Streamers;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class StreamerService(IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IStreamerService
    {
        public async Task<IEnumerable<StreamerModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            var streamers = await context.Streamers
                    .Select(c => mapper.Map<StreamerModel>(c))
                    .ToListAsync(cancellationToken);

            return streamers;
        }

        public async Task<IEnumerable<StreamerModel>> GetAllByUserAsync(CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            var streamers = await context.UserStreamers
                .Include(us => us.Streamer)
                .Where(us => us.UserId == userId)
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

        public async Task<bool> AddAsync(StreamerModel streamer, CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existingStreamer = await context.Streamers
                .Where(c => c.Id == streamer.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var isNew = false;

            if(existingStreamer == null)
            {
                context.Streamers.Add(mapper.Map<Streamer>(streamer));
                await context.SaveChangesAsync(cancellationToken);
                isNew = true;   
            }

            context.UserStreamers.Add(new UserStreamer
            {
                Id = Guid.NewGuid().ToString(),
                UserId = httpContextAccessor.HttpContext.User.GetUserId(),
                StreamerId = streamer.Id
            });

            await context.SaveChangesAsync(cancellationToken);

            return isNew;
        }

        public async Task<bool> AddAsync(StreamerDataModel streamer, CancellationToken cancellationToken = default)
        {
            return await AddAsync(mapper.Map<StreamerModel>(streamer), cancellationToken);
        }

        public async Task<StreamerDataModel> GetStreamerFromTwitchAsync(string username, CancellationToken cancellationToken = default)
        {
            var client = httpClientFactory.CreateClient("TwitchApi");

            var request = new GetStreamerHttpRequestMessage(login: username);

            var response = await client.SendAsync(request, cancellationToken);

            var streamers = await request.GetDataAsync(response, cancellationToken);

            return streamers.Data.FirstOrDefault();
        }

        public async Task UpdateAsync(StreamerModel streamer, CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            var existingStreamer = await context.UserStreamers
                .Where(us => us.UserId == userId && us.StreamerId == streamer.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if(existingStreamer == null)
            {
                return;
            }
            
            existingStreamer.IsClipDefault = streamer.IsClipDefault;

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
