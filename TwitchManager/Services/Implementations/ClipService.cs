using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.Extensions.ExpressionMapping.Impl;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using System.Linq.Dynamic.Core;

using TwitchManager.Comunications.TwicthApi.Api.Clips;
using TwitchManager.Comunications.TwicthApi.Api.Games;
using TwitchManager.Comunications.TwitchGQL.Clips;
using TwitchManager.Data.DbContexts;
using TwitchManager.Data.Domains;
using TwitchManager.Helpers;
using TwitchManager.Models.Clips;
using TwitchManager.Models.General;
using TwitchManager.Models.Streamers;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public partial class ClipService(ILogger<ClipService> logger,IOptionsMonitor<ConfigData> configMonitor, IDbContextFactory<TwitchManagerDbContext> dbContextFactory, IMapper mapper,
        IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider) : IClipService
    {
        private readonly IDataProtector _protector = provider.CreateProtector("DownloadController");

        public async Task<ICollection<ClipModel>> GetAllAsync()
        {
            var context = await dbContextFactory.CreateDbContextAsync();
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            var clips = await context.Clips
                .Include(c => c.Game)
                .Include(c => c.ClipVotes)
                .Select(c => mapper.Map<ClipModel>(c, opt => SetVotesInfo(opt, userId)))
                .ToListAsync();

            return clips;
        }

        private static void SetVotesInfo(IMappingOperationOptions<object, ClipModel> opt, string userId)
        {
            opt.AfterMap((src, dest) =>
            {
                var clip = (Clip)src;
                dest.IsUserVoted = clip.ClipVotes.Any(v => v.UserId == userId);
                dest.Votes = clip.ClipVotes.Count;
            });
        }

        public async Task<string> GetDownloadLinkAsync(string cliId, CancellationToken cancellationToken)
        {
            try
            {
                var client = httpClientFactory.CreateClient("TwitchGQL");
                var request = new ClipTokenTwitchGQLRequest(cliId);

                var httpResponse = await client.SendAsync(request, cancellationToken);
                var clipToken = (await request.GetDataAsync(httpResponse, cancellationToken)).FirstOrDefault() ?? throw new Exception("No video found in the clip");

                return clipToken.GetUrl();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error while getting download link for clip {ClipId}", cliId);

                throw new BadHttpRequestException("Error while getting download link for clip");
            }
        }

        public Task<string> GetDownloadLinkAsync(ClipModel clip, CancellationToken cancellationToken)
        {
            if(!clip.IsDownloaded)
                return GetDownloadLinkAsync(clip.Id, cancellationToken);

            var path = $"{configMonitor.CurrentValue.ClipDownloadPath}/{clip.BroadcasterId}/{clip.Id}.mp4";
            var protectedPath = _protector.Protect($"{DateTime.UtcNow.AddMinutes(5)}|{path}");

            return Task.FromResult($"/download/{protectedPath}");
        }

        public async Task<ClipModel> GetByIdAsync(string id)
        {
            var context = await dbContextFactory.CreateDbContextAsync();
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            var clip = await context.Clips
                .Include(c => c.Game)
                .Include(c => c.ClipVotes)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            return mapper.Map<ClipModel>(clip, opt => SetVotesInfo(opt, userId));
        }

        public async Task GetFromApiAsync(string streamerId, CancellationToken cancellationToken = default)
        {
            var client = httpClientFactory.CreateClient("TwitchApi");

            var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var maxDate = await context.Clips
                .Where(c => c.BroadcasterId == streamerId)  
                .MaxAsync(c => (DateTime?)c.CreatedAt, cancellationToken);

            //var from = maxDate == null ? "" : maxDate.Value.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var cursor = "";

            var today = DateTime.UtcNow.Date;
            var currentDate = maxDate != null ? maxDate.Value.Date : today;

            var isPremium = await context.Streamers.Where(s => s.Id == streamerId).Select(s => s.IsPremium).FirstOrDefaultAsync(cancellationToken);

            do
            {
                var from = currentDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
                var request = new GetClipsHttpRequestMessage(broadcasterId: streamerId, startedAt: from, after: cursor);

                var response = await client.SendAsync(request, cancellationToken);

                var clipResponse = await request.GetDataAsync(response, cancellationToken);

                var clips = clipResponse.Data.Select(mapper.Map<Clip>);

                foreach(var clip in clips)
                {
                    var existingGame = await context.Games.FindAsync([clip.GameId, cancellationToken], cancellationToken: cancellationToken);
                    if(existingGame == null)
                    {
                        var gameRequest = new GetGameHttpRequestMessage(id: clip.GameId);

                        var responseMessage = await client.SendAsync(gameRequest, cancellationToken);

                        var gameResponse = await gameRequest.GetDataAsync(responseMessage, cancellationToken);

                        var games = gameResponse.Data.Select(mapper.Map<Game>);

                        if (games.Any())
                        {
                            var game = games.First();
                            try
                            {
                                context.Games.Add(game);

                                await context.SaveChangesAsync(cancellationToken);
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
                        var exists = await context.Clips.AnyAsync(c => c.Id == clip.Id, cancellationToken);

                        if (exists)
                        {
                            continue;
                        }

                        context.Clips.Add(clip);

                        await context.SaveChangesAsync(cancellationToken);
                    }
                    catch
                    {
                    }
                }

                currentDate = currentDate.AddDays(1);

                cursor = clipResponse.Pagination.Cursor;
            }
            while (!string.IsNullOrEmpty(cursor) || currentDate != today);

            if (!isPremium)
            {
                logger.LogInformation("Streamer {StreamerId} is not premium, skipping clip download", streamerId);
                return;
            }

            var totalDownloaded = await context.Clips.Where(c => c.BroadcasterId == streamerId && c.IsDownloaded).CountAsync(cancellationToken);

            var maxDownloads = configMonitor.CurrentValue.MaxDownloadClips;

            if(totalDownloaded >= maxDownloads)
            {
                logger.LogInformation("Streamer {StreamerId} has already {TotalDownloaded}/{MaxDownloads} downloaded clips, skipping download", streamerId, totalDownloaded, maxDownloads);
                return;
            }

            var downloadDirectory = $"{configMonitor.CurrentValue.ClipDownloadPath}/{streamerId}";

            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            var clipsToDownload = await context.RandomClips
                .AsNoTracking()
                .Where(c => c.BroadcasterId == streamerId && !c.IsDownloaded)
                .OrderByDescending(c => c.CreatedAt)
                .Take(maxDownloads - totalDownloaded)
                .ToListAsync(cancellationToken);

            foreach (var clip in clipsToDownload)
            {
                try
                {
                    var downloadLink = await GetDownloadLinkAsync(clip.Id, cancellationToken);

                    var httpClient = new HttpClient();

                    using (var fileStream = File.OpenWrite($"{downloadDirectory}/{clip.Id}.mp4"))
                    {
                        var stream = await httpClient.GetStreamAsync(downloadLink, cancellationToken);

                        await stream.CopyToAsync(fileStream, cancellationToken);
                    }

                    await context.Clips
                        .Where(c => c.Id == clip.Id)
                        .ExecuteUpdateAsync(c => c.SetProperty(p => p.IsDownloaded, true), cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while downloading clip {ClipId}", clip.Id);
                }
            }

        }

        public async Task<ClipFilterResultModel> GetByStreamerAsync(ClipFilterModel filterModel, CancellationToken cancellationToken = default)
        {
            var result = new ClipFilterResultModel();
            try
            {

                var (clips, total) = await Task.Run(() =>
                {
                    var context = dbContextFactory.CreateDbContext();
                    var userId = httpContextAccessor.HttpContext.User.GetUserId();

                    IQueryable<ClipModel> query;

                    if(filterModel.IsRandom)
                    {
                        query = context.RandomClips
                        .Where(c => c.BroadcasterId == filterModel.StreamerId)
                        .Include(c => c.Game)   
                        .UseAsDataSource(mapper).For<ClipModel>();
                    }
                    else
                    {
                        query = context.Clips
                        .Where(c => c.BroadcasterId == filterModel.StreamerId)
                        .UseAsDataSource(mapper).For<ClipModel>()
                        .OnEnumerated(c =>
                        {
                            foreach (var clip in c.Cast<ClipModel>())
                            {
                                clip.IsUserVoted = context.ClipVotes.Any(v => v.ClipId == clip.Id && v.UserId == userId);
                            }
                        });
                    }

                    if (filterModel.Filters != null)
                    {
                        foreach (var filter in filterModel.Filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    if(filterModel.IsDownloaded.HasValue)
                    {
                        query = query.Where(c => c.IsDownloaded == filterModel.IsDownloaded.Value);
                    }

                    var total = query.Count(); 

                    if (!string.IsNullOrEmpty(filterModel.OrderBy))
                    {
                        query = query.OrderBy(filterModel.OrderBy);
                    }

                    var clips = query.Skip(filterModel.Skip).Take(filterModel.Take).ToList();

                    return (clips, total);
                }, cancellationToken);

                result.Clips = clips;   
                result.FilteredTotal = total;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error while getting clips");
            }

            return result;

        }

        public async Task VoteAsync(string clipId, bool isUpVote)
        {
            var userId = httpContextAccessor.HttpContext.User.GetUserId();

            await VoteAsync(clipId, userId, isUpVote);

        }

        public async Task VoteAsync(string clipId, string userId, bool isUpVote)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            if (isUpVote)
            {
                var upVote = new ClipVote
                {
                    Id = Guid.NewGuid().ToString(),
                    ClipId = clipId,
                    UserId = userId
                };

                context.ClipVotes.Add(upVote);
            }
            else
            {
                var vote = await context.ClipVotes
                    .Where(v => v.ClipId == clipId && v.UserId == userId)
                    .FirstOrDefaultAsync();

                if (vote != null)
                {
                    context.ClipVotes.Remove(vote);
                }
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while voting");
                throw new Exception("Error while voting", e);
            }
        }

        public async Task VoteAsync(string clipId, string userId)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var user = await context.Users.Where(u => u.TwitchId == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    TwitchId = userId,
                    CreatedOn = DateTime.Now
                };

                context.Users.Add(user);

                await context.SaveChangesAsync();
            }

            userId = user.Id;

            var vote = await context.ClipVotes
                .Where(v => v.ClipId == clipId && v.UserId == userId)
                .FirstOrDefaultAsync();

            if (vote != null)
            {
                context.ClipVotes.Remove(vote);
            }
            else
            {
                vote = new ClipVote
                {
                    Id = Guid.NewGuid().ToString(),
                    ClipId = clipId,
                    UserId = userId
                };

                context.ClipVotes.Add(vote);
            }

            await context.SaveChangesAsync();
        }

        public async Task<int> GetTotalByStremaerAsync(string streamerId)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var total = await context.Clips
                .Where(c => c.BroadcasterId == streamerId)
                .CountAsync();

            return total;
        }

        public async Task<ICollection<string>> GetCreatorsByStremaerAsync(string streamerId)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var creators = await context.Clips
                .Where(c => c.BroadcasterId == streamerId)
                .Select(c => c.CreatorName)
                .Distinct()
                .ToListAsync();

            return creators;
        }

        public async Task<ICollection<string>> GetGamesByStremaerAsync(string streamerId)
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var games = await context.Clips
                .Where(c => c.BroadcasterId == streamerId)
                .Select(c => c.Game.Name)
                .Distinct()
                .ToListAsync();

            return games;
        }
    }
}
