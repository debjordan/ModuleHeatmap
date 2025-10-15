namespace ModuleHeatMap.SDK.Models;

public enum AccessType
{
    View = 1,
    Click = 2,
    Form = 3,
    Download = 4,
    Upload = 5,
    Search = 6,
    Export = 7,
    Import = 8,
    Custom = 99
}

public class TrackingRequest
{
    public string ApplicationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleUrl { get; set; } = string.Empty;
    public AccessType AccessType { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
