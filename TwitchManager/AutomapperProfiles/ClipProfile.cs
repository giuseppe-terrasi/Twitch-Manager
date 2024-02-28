using AutoMapper;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Api.Games.Data;
using TwitchManager.Models.Clips;

namespace TwitchManager.AutomapperProfiles
{
    public class ClipProfile : Profile
    {
        public ClipProfile()
        {
            CreateMap<ClipDataModel, Data.Domains.Clip>()
                .ForMember(c => c.Game, opt => opt.Ignore())
                .ForMember(c => c.Streamer, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<GameDataModel, Data.Domains.Game>()
                .ForMember(g => g.Clips, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<ClipModel, Data.Domains.Clip>()
                .ForMember(c => c.CreatedAt, opt => opt.MapFrom(c => DateTime.SpecifyKind(c.CreatedAt, DateTimeKind.Local).ToUniversalTime()))
                .ReverseMap()
                .ForMember(c => c.GameName, opt => opt.MapFrom(c => c.Game.Name))
                .ForMember(c => c.CreatedAt, opt => opt.MapFrom(c => DateTime.SpecifyKind(c.CreatedAt, DateTimeKind.Utc).ToLocalTime()));
        }
    }
}
