using Microsoft.EntityFrameworkCore;
using ModuleHeatMap.Core.Entities;
using ModuleHeatMap.Core.Enums;
using ModuleHeatMap.Core.Interfaces;
using ModuleHeatMap.Core.ValueObjects;
using ModuleHeatMap.Infrastructure.Data;

namespace ModuleHeatMap.Infrastructure.Repositories;

public class ModuleAccessRepository : IModuleAccessRepository
{
    private readonly ApplicationDbContext _context;

    public ModuleAccessRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleAccess> AddAsync(ModuleAccess moduleAccess)
    {
        _context.ModuleAccesses.Add(moduleAccess);
        await _context.SaveChangesAsync();
        return moduleAccess;
    }

    public async Task<IEnumerable<ModuleAccess>> GetByApplicationAsync(string applicationId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ModuleAccesses
            .Where(ma => ma.ApplicationId == applicationId);

        if (startDate.HasValue)
            query = query.Where(ma => ma.AccessedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ma => ma.AccessedAt <= endDate.Value);

        return await query
            .OrderByDescending(ma => ma.AccessedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ModuleAccess>> GetByModuleAsync(string applicationId, string moduleName, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ModuleAccesses
            .Where(ma => ma.ApplicationId == applicationId && ma.ModuleName == moduleName);

        if (startDate.HasValue)
            query = query.Where(ma => ma.AccessedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ma => ma.AccessedAt <= endDate.Value);

        return await query
            .OrderByDescending(ma => ma.AccessedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ModuleAccess>> GetByUserAsync(string applicationId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ModuleAccesses
            .Where(ma => ma.ApplicationId == applicationId && ma.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(ma => ma.AccessedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(ma => ma.AccessedAt <= endDate.Value);

        return await query
            .OrderByDescending(ma => ma.AccessedAt)
            .ToListAsync();
    }

    public async Task<AccessMetrics> GetModuleMetricsAsync(string applicationId, string moduleName, DateTime startDate, DateTime endDate)
    {
        var accesses = await _context.ModuleAccesses
            .Where(ma => ma.ApplicationId == applicationId &&
                        ma.ModuleName == moduleName &&
                        ma.AccessedAt >= startDate &&
                        ma.AccessedAt <= endDate)
            .ToListAsync();

        if (!accesses.Any())
        {
            return new AccessMetrics
            {
                TotalAccesses = 0,
                UniqueUsers = 0,
                AverageSessionDuration = TimeSpan.Zero,
                FirstAccess = DateTime.MinValue,
                LastAccess = DateTime.MinValue,
                AccessFrequency = 0,
                AccessTypeDistribution = new Dictionary<AccessType, int>()
            };
        }

        var uniqueUsers = accesses.Select(a => a.UserId).Distinct().Count();
        var totalAccesses = accesses.Count;
        var averageDuration = accesses.Average(a => a.Duration.TotalSeconds);
        var firstAccess = accesses.Min(a => a.AccessedAt);
        var lastAccess = accesses.Max(a => a.AccessedAt);
        var daysDiff = Math.Max(1, (endDate - startDate).TotalDays);
        var accessFrequency = totalAccesses / daysDiff;

        var accessTypeDistribution = accesses
            .GroupBy(a => a.AccessType)
            .ToDictionary(g => g.Key, g => g.Count());

        return new AccessMetrics
        {
            TotalAccesses = totalAccesses,
            UniqueUsers = uniqueUsers,
            AverageSessionDuration = TimeSpan.FromSeconds(averageDuration),
            FirstAccess = firstAccess,
            LastAccess = lastAccess,
            AccessFrequency = accessFrequency,
            AccessTypeDistribution = accessTypeDistribution
        };
    }

    public async Task<IEnumerable<HeatMapData>> GetHeatMapDataAsync(string applicationId, DateTime startDate, DateTime endDate)
    {
        var accesses = await _context.ModuleAccesses
            .Where(ma => ma.ApplicationId == applicationId &&
                        ma.AccessedAt >= startDate &&
                        ma.AccessedAt <= endDate)
            .ToListAsync();

        var moduleGroups = accesses.GroupBy(a => a.ModuleName);
        var heatMapData = new List<HeatMapData>();

        foreach (var group in moduleGroups)
        {
            var moduleAccesses = group.ToList();
            var totalAccesses = moduleAccesses.Count;
            var uniqueUsers = moduleAccesses.Select(a => a.UserId).Distinct().Count();

            // Cálculo do HeatScore (0-100) baseado em acessos e usuários únicos
            var maxAccesses = moduleGroups.Max(g => g.Count());
            var maxUsers = moduleGroups.Max(g => g.Select(a => a.UserId).Distinct().Count());

            var accessScore = maxAccesses > 0 ? (double)totalAccesses / maxAccesses * 50 : 0;
            var userScore = maxUsers > 0 ? (double)uniqueUsers / maxUsers * 50 : 0;
            var heatScore = (int)Math.Round(accessScore + userScore);

            var metrics = new AccessMetrics
            {
                TotalAccesses = totalAccesses,
                UniqueUsers = uniqueUsers,
                AverageSessionDuration = TimeSpan.FromSeconds(moduleAccesses.Average(a => a.Duration.TotalSeconds)),
                FirstAccess = moduleAccesses.Min(a => a.AccessedAt),
                LastAccess = moduleAccesses.Max(a => a.AccessedAt),
                AccessFrequency = totalAccesses / Math.Max(1, (endDate - startDate).TotalDays),
                AccessTypeDistribution = moduleAccesses
                    .GroupBy(a => a.AccessType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            var topUsers = moduleAccesses
                .GroupBy(a => a.UserId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            var hourlyDistribution = moduleAccesses
                .GroupBy(a => a.AccessedAt.Hour.ToString("00"))
                .ToDictionary(g => g.Key, g => g.Count());

            // Busca informações do módulo se existir
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.ApplicationId == applicationId && m.Name == group.Key);

            heatMapData.Add(new HeatMapData
            {
                ModuleName = group.Key,
                ModuleDisplayName = module?.DisplayName ?? group.Key,
                Category = module?.Category ?? "Outros",
                HeatScore = heatScore,
                Metrics = metrics,
                TopUsers = topUsers,
                HourlyDistribution = hourlyDistribution
            });
        }

        return heatMapData.OrderByDescending(h => h.HeatScore);
    }
}
