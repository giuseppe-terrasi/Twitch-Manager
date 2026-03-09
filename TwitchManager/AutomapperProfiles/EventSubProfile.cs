using AutoMapper;

using TwitchManager.Data.Domains;
using TwitchManager.Models.Api.Events;

namespace TwitchManager.AutomapperProfiles
{
    public class EventSubProfile : Profile
    {
        public EventSubProfile()
        {
            CreateMap<EventSub, EventSubModel>()
                .ForMember(dest => dest.BotUserId, opt => opt.MapFrom(src => src.BotUser != null ? src.BotUser.TwitchId : ""))
                .ForMember(dest => dest.BotUsername, opt => opt.MapFrom(src => src.BotUser != null ? src.BotUser.TwitchUsername : ""))
                .ForMember(dest => dest.StreamerName, opt => opt.MapFrom(src => src.Streamer.DisplayName));
        }
    }
}
