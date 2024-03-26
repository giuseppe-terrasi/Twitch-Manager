﻿using AutoMapper;

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
                .ReverseMap();

            CreateMap<Data.Domains.UserStreamer, StreamerModel>()
                .ForMember(m => m.Id, opt => opt.MapFrom(us => us.Streamer.Id))
                .IncludeMembers(us => us.Streamer);
        }
    }
}
