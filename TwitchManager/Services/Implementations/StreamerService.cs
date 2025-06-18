using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Threading;

using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Helpers;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.General;
using TwitchManager.Models.Streamers;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class StreamerService(IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper, 
        IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ConfigData> optionsMonitor) : IStreamerService
    {

        public string GetStreamerIdByHost()
        {
            var host = httpContextAccessor.HttpContext.Request.Host.Host.ToString();
            var mappings = optionsMonitor.CurrentValue.HostStreamerMappings;
            var mapping = mappings.FirstOrDefault(m => m.Host == host);

            return mapping?.StreamerId;
        }

        public bool IsStreamerSetByHost()
        {
            var host = httpContextAccessor.HttpContext.Request.Host.Host.ToString();
            var mappings = optionsMonitor.CurrentValue.HostStreamerMappings;
            var mapping = mappings.FirstOrDefault(m => m.Host == host);

            return mapping != null;
        }

        public async Task<StreamerModel> GetByHostAsync()
        {
            var host = httpContextAccessor.HttpContext.Request.Host.Host.ToString();

            var mappings = optionsMonitor.CurrentValue.HostStreamerMappings;

            var mapping = mappings.FirstOrDefault(m => m.Host == host);
            if (mapping == null)
            {
                return null;
            }

            var context = await dbContextFactory.CreateDbContextAsync();

            var streamer = await context.Streamers
                .Where(c => c.Id == mapping.StreamerId)
                .Select(c => mapper.Map<StreamerModel>(c))
                .FirstOrDefaultAsync();

            return streamer;

        }

        public async Task<ICollection<StreamerModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var userId = httpContextAccessor.HttpContext?.User?.GetUserId();

            var query = context.Streamers.AsQueryable();

            if(userId is not null)
            {
                query = query
                    .Include(s => s.UserStreamers.Where(s => s.UserId == userId && s.IsClipDefault));
            }


            var streamers = await query.Select(c => mapper.Map<StreamerModel>(c))
                    .ToListAsync(cancellationToken);

            return streamers;
        }

        public async Task<ICollection<StreamerModel>> GetAllByUserAsync(CancellationToken cancellationToken = default)
        {
            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            var query = context.UserStreamers
                .Include(us => us.Streamer)
                .AsQueryable();

            if(!string.IsNullOrEmpty(userId))
            {
                query = query.Where(us => us.UserId == userId);
            }

            var streamers = await query
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

        public async Task<bool> UserHasAnyStreamer()
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            return await context.UserStreamers
                .AnyAsync(us => us.UserId == userId);
        }
    }
}
