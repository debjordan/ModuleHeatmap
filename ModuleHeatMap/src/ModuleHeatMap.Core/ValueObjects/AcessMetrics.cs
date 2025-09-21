using ModuleHeatMap.Core.Enums;

namespace ModuleHeatMap.Core.ValueObjects;

public record AccessMetrics
{
    public int TotalAccesses { get; init; }
    public int UniqueUsers { get; init; }
    public TimeSpan AverageSessionDuration { get; init; }
    public DateTime FirstAccess { get; init; }
    public DateTime LastAccess { get; init; }
    public double AccessFrequency { get; init; }
    public Dictionary<AccessType, int> AccessTypeDistribution { get; init; } = []; // view, forms, clicks, etc
}
