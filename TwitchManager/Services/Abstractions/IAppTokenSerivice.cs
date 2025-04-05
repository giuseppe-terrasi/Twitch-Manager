namespace TwitchManager.Services.Abstractions
{
    public interface IAppTokenSerivice
    {
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
    }
}
