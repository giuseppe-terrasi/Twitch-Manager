using AutoMapper;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Api.Games.Data;

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
        }
    }
}
