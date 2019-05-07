using AutoMapper;
using CheckerApi.Models.Config;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Models.Rpc;
using System;

namespace CheckerApi.AutoMapper
{
    public class ApiProfile : Profile
    {
        public ApiProfile()
        {
            CreateMap<BidDTO, BidEntry>()
                .ForMember(
                    dest => dest.ID,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.NiceHashId,
                    opts => opts.MapFrom(src => src.Id))
                .ForMember(
                    dest => dest.RecordDate,
                    opts => opts.MapFrom(src => DateTime.UtcNow))
                ;

            CreateMap<BidEntry, BidAudit>()
                ;

            CreateMap<PoolHashrate, PoolHashrateDTO>()
                .ForMember(
                    dest => dest.Date,
                    opt => opt.MapFrom(src => src.EntryDate))
                .ForMember(
                    dest => dest.Value,
                    opt => opt.MapFrom(src => src.Value * 1000)) // To KSol
                .ForMember(
                    dest => dest.Denomination,
                    opt => opt.MapFrom(src => Denomination.Ksol.ToString()))
                ;

            CreateMap<RpcBlockResult, BlockInfoDTO>()
                .ForMember(
                    dest => dest.Hash,
                    opt => opt.MapFrom(src => src.Result.Hash))
                .ForMember(
                    dest => dest.Height,
                    opt => opt.MapFrom(src => int.Parse(src.Result.Height)))
                .ForMember(
                    dest => dest.Time,
                    opt => opt.MapFrom(src => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(long.Parse(src.Result.Time))))
                .ForMember(
                    dest => dest.PreviousBlockHash,
                    opt => opt.MapFrom(src => src.Result.PreviousBlockHash))
                ;
        }
    }
}
