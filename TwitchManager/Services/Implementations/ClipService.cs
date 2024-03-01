using AutoMapper;
using Microsoft.EntityFrameworkCore;

using System.Linq.Dynamic.Core;

using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Comunications.TwicthApi.Api.Games;
using TwitchManager.Comunications.TwitchGQL.Clips;
using TwitchManager.Data;
using TwitchManager.Data.Domains;
using TwitchManager.Models.Clips;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public partial class ClipService(IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper, IHttpClientFactory httpClientFactory) : IClipService
    {
        public async Task<IEnumerable<ClipModel>> GetAllAsync()
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var clips = await context.Clips
                .Include(c => c.Game)
                .Select(c => mapper.Map<ClipModel>(c))
                .ToListAsync();

            return clips;
        }

        public async Task<string> GetDownloadLinkAsync(string cliId, CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient("TwitchGQL");
            var request = new ClipTokenTwitchGQLRequest(cliId);

            var httpResponse = await client.SendAsync(request, cancellationToken);  
            var clipToken = (await request.GetDataAsync(httpResponse)).FirstOrDefault() ?? throw new Exception("No video found in the clip");

            return clipToken.GetUrl();
        }

        public async Task<ClipModel> GetByIdAsync(string id)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var clip = await context.Clips
                .Include(c => c.Game)
                .Where(c => c.Id == id) 
                .Select(c => mapper.Map<ClipModel>(c))
                .FirstOrDefaultAsync();

            return clip;
        }

        public async Task GetFromApiAsync(string streamerId)
        {
            var client = httpClientFactory.CreateClient("TwitchApi");

            var context = await dbContextFactory.CreateDbContextAsync();

            var maxDate = await context.Clips
                .Where(c => c.BroadcasterId == streamerId)  
                .MaxAsync(c => (DateTime?)c.CreatedAt);

            var from = maxDate == null ? "" : maxDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var cursor = "";

            do
            {
                var request = new GetClipsHttpRequestMessage(broadcasterId: streamerId, first: "100", startedAt: from, after: cursor);

                var response = await client.SendAsync(request);

                var clipResponse = await request.GetDataAsync(response);

                var clips = clipResponse.Data.Select(mapper.Map<Clip>);

                foreach(var clip in clips)
                {
                    var existingGame = await context.Games.FindAsync(clip.GameId);
                    if(existingGame == null)
                    {
                        var gameRequest = new GetGameHttpRequestMessage(id: clip.GameId);

                        var responseMessage = await client.SendAsync(gameRequest);

                        var gameResponse = await gameRequest.GetDataAsync(responseMessage);

                        var games = gameResponse.Data.Select(mapper.Map<Game>);

                        if (games.Any())
                        {
                            var game = games.First();
                            try
                            {
                                context.Games.Add(game);

                                await context.SaveChangesAsync();
                            }
                            catch
                            {
                                context.Entry(game).State = EntityState.Detached;
                                continue;
                            }
                        }
                    }

                    try
                    {
                        context.Clips.Add(clip);

                        await context.SaveChangesAsync();
                    }
                    catch
                    {
                        context.Entry(clip).State = EntityState.Detached;
                    }
                }

                cursor = clipResponse.Pagination.Cursor;
            }
            while (!string.IsNullOrEmpty(cursor));
            
        }

        public async Task<IEnumerable<ClipModel>> GetByStreamerAsync(string streamerId)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var clips = await context.Clips
                .Include(c => c.Game)
                .Where(c => c.BroadcasterId == streamerId)
                .Select(c => mapper.Map<ClipModel>(c))
                .ToListAsync();

            return clips;
        }
    }
}
