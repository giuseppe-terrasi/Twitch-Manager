using AutoMapper;

using TwitchManager.Data.Domains;
using TwitchManager.Helpers;
using TwitchManager.Models.Api.Clips.Data;
using TwitchManager.Models.Api.Games.Data;
using TwitchManager.Models.Clips;

namespace TwitchManager.AutomapperProfiles
{
    public class ClipProfile : Profile
    {
        
        public ClipProfile()
        {
            
            CreateMap<ClipDataModel, Clip>()
                .ForMember(c => c.Game, opt => opt.Ignore())
                .ForMember(c => c.Streamer, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<GameDataModel, Game>()
                .ForMember(g => g.Clips, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<ClipModel, Clip>()
                .ReverseMap()
                //.ForMember(c => c.IsUserVoted, opt => opt.MapFrom((c,m) => c.ClipVotes.Any(c => c.UserId == "")))
                .ForMember(c => c.Votes, opt => opt.MapFrom(c => c.ClipVotes.Count))
                .ForMember(c => c.GameName, opt => opt.MapFrom(c => c.Game.Name));
        }

    }
}
