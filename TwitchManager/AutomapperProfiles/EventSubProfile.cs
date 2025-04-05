using AutoMapper;

using TwitchManager.Data.Domains;
using TwitchManager.Models.Api.Events;

namespace TwitchManager.AutomapperProfiles
{
    public class EventSubProfile : Profile
    {
        public EventSubProfile()
        {
            CreateMap<EventSub, EventSubModel>();
        }
    }
}
