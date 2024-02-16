using AutoMapper;
using Microsoft.EntityFrameworkCore;

using OpenQA.Selenium.Chrome;

using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;

using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Comunications.TwicthApi.Api.Games;
using TwitchManager.Data;
using TwitchManager.Data.Domains;
using TwitchManager.Models.Clips;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public partial class ClipService(IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper, IHttpClientFactory httpClientFactory) : IClipService, IDisposable
    {
        private ChromeDriver chromeDriver = null;

        public async Task<IEnumerable<ClipModel>> GetAllAsync()
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var clips = await context.Clips
                .Include(c => c.Game)
                .Select(c => mapper.Map<ClipModel>(c))
                .ToListAsync();

            return clips;
        }

        public async Task<string> GetDownloadLinkAsync(string clipUrl, CancellationToken cancellationToken)
        {
            var result = await Task.Run(() =>
            {
                if(chromeDriver == null)
                {
                    CreateChromeDriver();
                }

                chromeDriver.Navigate().GoToUrl(clipUrl);

                var html = chromeDriver.PageSource;

                var match = VideoRegex().Match(html);

                if (match.Success)
                {
                    var videoTag = match.Value;

                    var videoUrl = videoTag.Split("src=\"")[1].Split("\"")[0].Replace("&amp;", "&");

                    return videoUrl;
                }

                throw new Exception("No video found in the clip");
            }, cancellationToken);

            return result;
        }

        [GeneratedRegex("<video.*></video>")]
        private static partial Regex VideoRegex();

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

            var missingGameIds = await context.Clips
                .Where(c => c.BroadcasterId == streamerId && !context.Games.Any(g => g.Id == c.GameId))
                .Select(c => c.GameId)
                .Distinct()
                .ToListAsync();

            foreach(var gameId in missingGameIds)
            {
                var request = new GetGameHttpRequestMessage(id: gameId);

                var response = await client.SendAsync(request);

                var gameResponse = await request.GetDataAsync(response);

                var games = gameResponse.Data.Select(mapper.Map<Game>);

                if(games.Any())
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
                    }
                }
            }
            
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

        public void CreateChromeDriver()
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless");
            options.AddArguments("--disable-gpu");

            chromeDriver = new ChromeDriver(options);
        }

        public void DisposeChromeDriver()
        {
            chromeDriver?.Dispose();
            chromeDriver = null;
        }

        public void Dispose()
        {
            DisposeChromeDriver();
            GC.SuppressFinalize(this);
        }
    }
}
