using TwitchManager.Data.Domains;

namespace TwitchManager.Models.Api.Events
{
    public class EventSubModel
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
    }
}
