using AutoMapper;

using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Streamers;

namespace TwitchManager.AutomapperProfiles
{
    public class StreamerProfile : Profile
    {
        public StreamerProfile()
        {
            CreateMap<StreamerDataModel, Data.Domains.Streamer>()
                .ReverseMap();

            CreateMap<StreamerDataModel, StreamerModel>()
                .ReverseMap();

            CreateMap<StreamerModel, Data.Domains.Streamer>()
                .ReverseMap()
                .ForMember(m => m.IsClipDefault, opt => opt.MapFrom(s => s.UserStreamers.Where(s => s.IsClipDefault).Any()));

            CreateMap<Data.Domains.UserStreamer, StreamerModel>()
                .ForMember(m => m.Id, opt => opt.MapFrom(us => us.Streamer.Id))
                .IncludeMembers(us => us.Streamer);
        }
    }
}
