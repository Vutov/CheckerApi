using System;
using AutoMapper;
using CheckerApi.DTO;

namespace CheckerApi
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<DataDTO, DataDB>()
                .ForMember(dest => dest.ID,
                    opt => opt.Ignore())
                .ForMember(dest => dest.NiceHashId,
                    opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.RecordDate,
                    opts => opts.MapFrom(src => DateTime.UtcNow))
                ;
        }
    }
}
