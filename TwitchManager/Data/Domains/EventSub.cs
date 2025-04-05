namespace TwitchManager.Data.Domains
{
    public enum EventSubStatus
    {
        Pending,
        RequestFailed,
        Enabled,
        Disabled,
        Revoked
    }

    public class EventSub
    {
        public string Id { get; set; }

        public string TwitchEventId { get; set; }

        public string Type { get; set; }

        public string Version { get; set; }

        public string StreamerId { get; set; }

        public string Method { get; set; }

        public string Callback { get; set; }

        public string Secret { get; set; }

        public string Condition { get; set; }

        public EventSubStatus Status { get; set; }

        public string RequestError { get; set; }

        public string Action { get; set; }

        public virtual Streamer Streamer { get; set; }
    }
}
