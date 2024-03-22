using Quartz;

using TwitchManager.Services.Abstractions;

namespace TwitchManager.Jobs
{
    public class ClipSyncronizerJob(ILogger<ClipSyncronizerJob> logger, IStreamerService streamerService, IClipService clipService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Starting ClipSyncronizerJob");

            try
            {
                var streamers = await streamerService.GetAllAsync(context.CancellationToken);

                foreach (var streamer in streamers)
                {
                    await clipService.GetFromApiAsync(streamer.Id, context.CancellationToken);
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error in ClipSyncronizerJob");
            }

            logger.LogInformation("ClipSyncronizerJob finished");
        }
    }
}
