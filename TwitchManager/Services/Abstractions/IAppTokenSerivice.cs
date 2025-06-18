namespace TwitchManager.Services.Abstractions
{
    public interface IAppTokenSerivice
    {
        public Task<string> GetAccessTokenAsync(string twitchId, CancellationToken cancellationToken);
    }
}
