using AutoMapper;

using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using TwitchManager.Data;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public class ClipService(IDbContextFactory<ClipManagerContext> dbContextFactory, IMapper mapper) : IClipService
    {

        public async Task<List<ClipDataModel>> GetAllAsync()
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var clips = await context.Clips
                .Select(c => mapper.Map<ClipDataModel>(c))
                .ToListAsync();

            return clips;
        }


    }
}
