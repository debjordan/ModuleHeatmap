namespace ModuleHeatMap.Application.DTOs;

public record AnalyticsRequestDto
{
    public string ApplicationId { get; init; } = string.Empty;
    public string? ModuleName { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? UserId { get; init; }
    public string? Category { get; init; }
}
