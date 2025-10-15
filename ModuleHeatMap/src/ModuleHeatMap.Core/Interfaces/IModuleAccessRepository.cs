using ModuleHeatMap.Core.Entities;
using ModuleHeatMap.Core.ValueObjects;

namespace ModuleHeatMap.Core.Interfaces;

public interface IModuleAccessRepository
{
    Task<ModuleAccess> AddAsync(ModuleAccess moduleAccess);
    Task<IEnumerable<ModuleAccess>> GetByApplicationAsync(string applicationId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ModuleAccess>> GetByModuleAsync(string applicationId, string moduleName, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ModuleAccess>> GetByUserAsync(string applicationId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AccessMetrics> GetModuleMetricsAsync(string applicationId, string moduleName, DateTime startDate, DateTime endDate);
    Task<IEnumerable<HeatMapData>> GetHeatMapDataAsync(string applicationId, DateTime startDate, DateTime endDate);
}
