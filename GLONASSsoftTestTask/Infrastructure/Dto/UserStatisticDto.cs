using AutoMapper;
using GLONASSsoftTestTask.Infrastructure.Models.Entities;
using System;

namespace GLONASSsoftTestTask.Infrastructure.Dto
{
    public class UserStatisticDto
    {
        public Guid UserId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
    public class UserStatisticProfile : Profile
    {
        public UserStatisticProfile()
        {
            CreateMap<UserStatisticDto, StatisticTaskEntity>()
                .ForMember(x => x.Created, opt => opt.MapFrom(y => DateTime.Now))
                .ForMember(x => x.DateStart, opt => opt.MapFrom(y => y.DateStart))
                .ForMember(x => x.Percent, opt => opt.Ignore())
                .ForMember(x => x.DateEnd, opt => opt.MapFrom(y => y.DateEnd))
                .ForMember(x => x.UserId, opt => opt.MapFrom(y=> y.UserId));

        }
    }
}
