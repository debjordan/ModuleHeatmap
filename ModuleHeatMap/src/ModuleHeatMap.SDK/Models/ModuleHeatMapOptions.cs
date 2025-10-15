namespace ModuleHeatMap.SDK.Models;

public class ModuleHeatMapOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableAutoTracking { get; set; } = false;
    public bool EnableBatchMode { get; set; } = false;
    public int BatchSize { get; set; } = 10;
    public TimeSpan BatchInterval { get; set; } = TimeSpan.FromMinutes(1);
}
