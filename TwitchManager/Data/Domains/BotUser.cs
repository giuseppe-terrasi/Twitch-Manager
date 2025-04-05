namespace TwitchManager.Data.Domains
{
    public class BotUser
    {
        public string Id { get; set; }

        public string TwitchId { get; set; }

        public DateTime CreatedOn { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string IdToken { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
