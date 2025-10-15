namespace ModuleHeatMap.SDK.Models;

public class BatchTrackingResult
{
    public List<BatchTrackingItem> Results { get; set; } = [];
}

public class BatchTrackingItem
{
    public string ModuleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public string? Error { get; set; }
}

public class HeatMapResponse
{
    public string ApplicationId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<ModuleHeatData> Modules { get; set; } = [];
    public HeatMapSummary Summary { get; set; } = null!;
}

public class ModuleHeatData
{
    public string ModuleName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int HeatScore { get; set; }
    public int TotalAccesses { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageSessionMinutes { get; set; }
    public string LastAccess { get; set; } = string.Empty;
    public IEnumerable<string> TopUsers { get; set; } = [];
    public Dictionary<string, int> HourlyDistribution { get; set; } = [];
}  

public class HeatMapSummary
{
    public int TotalModules { get; set; }
    public int ActiveModules { get; set; }
    public int UnusedModules { get; set; }
    public int TotalAccesses { get; set; }
    public int TotalUniqueUsers { get; set; }
    public string MostUsedModule { get; set; } = string.Empty;
    public string LeastUsedModule { get; set; } = string.Empty;
}

public class ModuleAnalyticsResponse
{
    public string ApplicationId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public AnalyticsPeriod Period { get; set; } = null!;
    public ModuleMetrics Metrics { get; set; } = null!;
}

public class AnalyticsPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ModuleMetrics
{
    public int TotalAccesses { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageSessionMinutes { get; set; }
    public DateTime FirstAccess { get; set; }
    public DateTime LastAccess { get; set; }
    public double AccessFrequency { get; set; }
    public Dictionary<string, int> AccessTypeDistribution { get; set; } = [];
}
