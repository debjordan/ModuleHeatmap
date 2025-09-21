namespace ModuleHeatMap.Core.ValueObjects;

public record HeatMapData
{
    public string ModuleName { get; init; } = string.Empty;
    public string ModuleDisplayName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int HeatScore { get; init; }
    public AccessMetrics Metrics { get; init; } = null!;
    public List<string> TopUsers { get; init; } = [];
    public Dictionary<string, int> HourlyDistribution { get; init; } = [];
}
