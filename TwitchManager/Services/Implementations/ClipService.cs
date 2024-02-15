using AutoMapper;

using HtmlAgilityPack;

using Microsoft.EntityFrameworkCore;

using OpenQA.Selenium.Chrome;

using System.Text.RegularExpressions;
using System.Web;

using TwitchManager.Data;
using TwitchManager.Models.Clips;
using TwitchManager.Services.Abstractions;

namespace TwitchManager.Services.Implementations
{
    public partial class ClipService(IDbContextFactory<ClipManagerContext> dbContextFactory, IMapper mapper) : IClipService
    {

        public async Task<List<ClipModel>> GetAllAsync()
        {
            var context = await dbContextFactory.CreateDbContextAsync();

            var clips = await context.Clips
                .Include(c => c.Game)
                .Select(c => mapper.Map<ClipModel>(c))
                .ToListAsync();

            return clips;
        }

        public async Task<string> GetDownloadLinkAsync(string clipUrl)
        {
            var result = await Task.Run(() =>
            {
                var options = new ChromeOptions();
                options.AddArguments("--headless");
                options.AddArguments("--disable-gpu");

                using var driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl(clipUrl);

                var html = driver.PageSource;

                var match = VideoRegex().Match(html);

                if (match.Success)
                {
                    var videoTag = match.Value;

                    var videoUrl = videoTag.Split("src=\"")[1].Split("\"")[0].Replace("&amp;", "&");

                    return videoUrl;
                }

                throw new Exception("No video found in the clip");
            });

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
    }
}
