using ModuleHeatMap.Core.Enums;
using ModuleHeatMap.Core.ValueObjects;

namespace ModuleHeatMap.Core.Interfaces;

public interface IModuleHeatMapService
{
    Task TrackModuleAccessAsync(string applicationId, string userId, string moduleName, string moduleUrl, AccessType accessType, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<HeatMapData>> GetHeatMapAsync(string applicationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AccessMetrics> GetModuleAnalyticsAsync(string applicationId, string moduleName, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<string>> GetUnusedModulesAsync(string applicationId, int daysSinceLastAccess = 30);
    Task<IEnumerable<string>> GetTopUsersForModuleAsync(string applicationId, string moduleName, int limit = 10);
}
