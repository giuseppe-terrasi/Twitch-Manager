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
                .ReverseMap();
            CreateMap<GameDataModel, Data.Domains.Game>()
                .ReverseMap();
            CreateMap<ClipModel, Data.Domains.Clip>()
                .ReverseMap()
                .ForMember(c => c.GameName, opt => opt.MapFrom(c => c.Game.Name));
        }
    }
}
