using ModuleHeatMap.Core.Entities;
using ModuleHeatMap.Core.Enums;
using ModuleHeatMap.Core.Interfaces;
using ModuleHeatMap.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ModuleHeatMap.Application.Services;

public class ModuleHeatMapService : IModuleHeatMapService
{
    private readonly IModuleAccessRepository _repository;
    private readonly ILogger<ModuleHeatMapService> _logger;

    public ModuleHeatMapService(IModuleAccessRepository repository, ILogger<ModuleHeatMapService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task TrackModuleAccessAsync(string applicationId, string userId, string moduleName,
        string moduleUrl, AccessType accessType, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var moduleAccess = new ModuleAccess
            {
                ApplicationId = applicationId,
                UserId = userId,
                ModuleName = moduleName,
                ModuleUrl = moduleUrl,
                AccessType = accessType,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            await _repository.AddAsync(moduleAccess);

            _logger.LogInformation("Module access tracked: {ModuleName} by {UserId} in {ApplicationId}",
                moduleName, userId, applicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking module access for {ModuleName} by {UserId}", moduleName, userId);
            throw;
        }
    }

    public async Task<IEnumerable<HeatMapData>> GetHeatMapAsync(string applicationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        return await _repository.GetHeatMapDataAsync(applicationId, start, end);
    }

    public async Task<AccessMetrics> GetModuleAnalyticsAsync(string applicationId, string moduleName, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        return await _repository.GetModuleMetricsAsync(applicationId, moduleName, start, end);
    }

    public async Task<IEnumerable<string>> GetUnusedModulesAsync(string applicationId, int daysSinceLastAccess = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastAccess);
        var recentAccesses = await _repository.GetByApplicationAsync(applicationId, cutoffDate);

        var accessedModules = recentAccesses.Select(a => a.ModuleName).Distinct().ToHashSet();

        var allAccesses = await _repository.GetByApplicationAsync(applicationId);
        var allModules = allAccesses.Select(a => a.ModuleName).Distinct();

        return allModules.Where(m => !accessedModules.Contains(m));
    }

    public async Task<IEnumerable<string>> GetTopUsersForModuleAsync(string applicationId, string moduleName, int limit = 10)
    {
        var accesses = await _repository.GetByModuleAsync(applicationId, moduleName, DateTime.UtcNow.AddDays(-30));

        return accesses
            .GroupBy(a => a.UserId)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => g.Key);
    }
}
