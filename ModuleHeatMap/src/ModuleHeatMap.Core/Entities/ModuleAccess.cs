using ModuleHeatMap.Core.Enums;

namespace ModuleHeatMap.Core.Entities;

public class ModuleAccess
{
    public Guid Id { get; set; }
    public string ApplicationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleUrl { get; set; } = string.Empty;
    public AccessType AccessType { get; set; }
    public DateTime AccessedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public ModuleAccess()
    {
        Id = Guid.NewGuid();
        AccessedAt = DateTime.UtcNow;
    }
}
