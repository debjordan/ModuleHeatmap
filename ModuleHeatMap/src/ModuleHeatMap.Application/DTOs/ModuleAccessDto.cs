using ModuleHeatMap.Core.Enums;

namespace ModuleHeatMap.Application.DTOs;

public record ModuleAccessDto
{
    public string ApplicationId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string ModuleName { get; init; } = string.Empty;
    public string ModuleUrl { get; init; } = string.Empty;
    public AccessType AccessType { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}
