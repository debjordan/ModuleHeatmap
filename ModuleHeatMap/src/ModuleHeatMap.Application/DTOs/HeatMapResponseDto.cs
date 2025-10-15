namespace ModuleHeatMap.Application.DTOs;

public record HeatMapResponseDto
{
    public string ApplicationId { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<ModuleHeatDataDto> Modules { get; init; } = [];
    public HeatMapSummaryDto Summary { get; init; } = null!;
}

public record ModuleHeatDataDto
{
    public string ModuleName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int HeatScore { get; init; }
    public int TotalAccesses { get; init; }
    public int UniqueUsers { get; init; }
    public double AverageSessionMinutes { get; init; }
    public string LastAccess { get; init; } = string.Empty;
    public IEnumerable<string> TopUsers { get; init; } = [];
    public Dictionary<string, int> HourlyDistribution { get; init; } = [];
}

public record HeatMapSummaryDto
{
    public int TotalModules { get; init; }
    public int ActiveModules { get; init; }
    public int UnusedModules { get; init; }
    public int TotalAccesses { get; init; }
    public int TotalUniqueUsers { get; init; }
    public string MostUsedModule { get; init; } = string.Empty;
    public string LeastUsedModule { get; init; } = string.Empty;
}
