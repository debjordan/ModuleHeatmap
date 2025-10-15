using AutoMapper;
using ModuleHeatMap.Application.DTOs;
using ModuleHeatMap.Core.Entities;
using ModuleHeatMap.Core.ValueObjects;

namespace ModuleHeatMap.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<ModuleAccessDto, ModuleAccess>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AccessedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Duration, opt => opt.Ignore())
            .ForMember(dest => dest.UserAgent, opt => opt.Ignore())
            .ForMember(dest => dest.IpAddress, opt => opt.Ignore());

        CreateMap<HeatMapData, ModuleHeatDataDto>()
            .ForMember(dest => dest.AverageSessionMinutes,
                opt => opt.MapFrom(src => src.Metrics.AverageSessionDuration.TotalMinutes))
            .ForMember(dest => dest.LastAccess,
                opt => opt.MapFrom(src => src.Metrics.LastAccess.ToString("yyyy-MM-dd HH:mm:ss")))
            .ForMember(dest => dest.TotalAccesses,
                opt => opt.MapFrom(src => src.Metrics.TotalAccesses))
            .ForMember(dest => dest.UniqueUsers,
                opt => opt.MapFrom(src => src.Metrics.UniqueUsers));
    }
}
