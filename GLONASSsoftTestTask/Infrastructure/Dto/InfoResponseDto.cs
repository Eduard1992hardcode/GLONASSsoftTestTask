using AutoMapper;
using GLONASSsoftTestTask.Infrastructure.Models.Entities;
using Newtonsoft.Json;
using System;

namespace GLONASSsoftTestTask.Infrastructure.Dto
{
    public class InfoResponseDto
    {
        [JsonProperty("query")]
        public Guid Query { get; set; }

        [JsonProperty("percent")]
        public int Percent { get; set; }

        [JsonProperty("result")]
        public InfoResultResponseDto Result { get; set; }
    }
    public class InfoResponseDtoProfile : Profile
    {
        public InfoResponseDtoProfile()
        {
            CreateMap<StatisticTaskEntity, InfoResponseDto > ()
                .ForMember(x => x.Query, opt => opt.MapFrom(y => y.Id))
                .ForMember(x => x.Percent, opt => opt.MapFrom(x => x.Percent))
                .ForMember(x => x.Result , opt => opt.Ignore());
        }
    }

    public class InfoResultResponseDto
    {
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        [JsonProperty("count_sign_in")]
        public int CountSignIn { get; set; }
    }
    public class InfoResultResponseDtoProfile : Profile
    {
        public InfoResultResponseDtoProfile()
        {
            CreateMap<StatisticTaskResultEntity, InfoResultResponseDto>()
                .ForMember(x => x.UserId, opt => opt.MapFrom(y => y.Task.UserId))
                .ForMember(x => x.CountSignIn, opt => opt.MapFrom(x => x.CountSignIn));
        }
    }
}

