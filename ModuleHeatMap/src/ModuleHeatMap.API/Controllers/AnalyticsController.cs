using Microsoft.AspNetCore.Mvc;
using ModuleHeatMap.Application.DTOs;
using ModuleHeatMap.Core.Interfaces;
using AutoMapper;

namespace ModuleHeatMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IModuleHeatMapService _heatMapService;
    private readonly IMapper _mapper;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IModuleHeatMapService heatMapService,
        IMapper mapper,
        ILogger<AnalyticsController> logger)
    {
        _heatMapService = heatMapService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o heatmap de módulos para uma aplicação
    /// </summary>
    [HttpGet("{applicationId}/heatmap")]
    public async Task<IActionResult> GetHeatMap(
        string applicationId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var heatMapData = await _heatMapService.GetHeatMapAsync(applicationId, start, end);
            var mappedData = _mapper.Map<IEnumerable<ModuleHeatDataDto>>(heatMapData);

            var summary = new HeatMapSummaryDto
            {
                TotalModules = mappedData.Count(),
                ActiveModules = mappedData.Count(m => m.TotalAccesses > 0),
                UnusedModules = mappedData.Count(m => m.TotalAccesses == 0),
                TotalAccesses = mappedData.Sum(m => m.TotalAccesses),
                TotalUniqueUsers = mappedData.SelectMany(m => m.TopUsers).Distinct().Count(),
                MostUsedModule = mappedData.OrderByDescending(m => m.TotalAccesses).FirstOrDefault()?.ModuleName ?? "",
                LeastUsedModule = mappedData.OrderBy(m => m.TotalAccesses).FirstOrDefault()?.ModuleName ?? ""
            };

            var response = new HeatMapResponseDto
            {
                ApplicationId = applicationId,
                StartDate = start,
                EndDate = end,
                Modules = mappedData,
                Summary = summary
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting heatmap for application {ApplicationId}", applicationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Obtém analytics detalhados de um módulo específico
    /// </summary>
    [HttpGet("{applicationId}/modules/{moduleName}/analytics")]
    public async Task<IActionResult> GetModuleAnalytics(
        string applicationId,
        string moduleName,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var analytics = await _heatMapService.GetModuleAnalyticsAsync(applicationId, moduleName, start, end);

            return Ok(new
            {
                applicationId,
                moduleName,
                period = new { startDate = start, endDate = end },
                metrics = analytics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for module {ModuleName} in application {ApplicationId}",
                moduleName, applicationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Obtém lista de módulos não utilizados
    /// </summary>
    [HttpGet("{applicationId}/unused-modules")]
    public async Task<IActionResult> GetUnusedModules(
        string applicationId,
        [FromQuery] int daysSinceLastAccess = 30)
    {
        try
        {
            var unusedModules = await _heatMapService.GetUnusedModulesAsync(applicationId, daysSinceLastAccess);

            return Ok(new
            {
                applicationId,
                daysSinceLastAccess,
                unusedModules = unusedModules.ToList(),
                count = unusedModules.Count(),
                recommendations = new[]
                {
                    "Considere descontinuar módulos não utilizados",
                    "Avalie se há necessidade de treinamento para estes módulos",
                    "Verifique se há problemas de usabilidade nos módulos abandonados"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused modules for application {ApplicationId}", applicationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Obtém os principais usuários de um módulo
    /// </summary>
    [HttpGet("{applicationId}/modules/{moduleName}/top-users")]
    public async Task<IActionResult> GetTopUsers(
        string applicationId,
        string moduleName,
        [FromQuery] int limit = 10)
    {
        try
        {
            var topUsers = await _heatMapService.GetTopUsersForModuleAsync(applicationId, moduleName, limit);

            return Ok(new
            {
                applicationId,
                moduleName,
                topUsers = topUsers.ToList(),
                recommendations = new[]
                {
                    "Usuários frequentes podem ser bons candidatos para feedback sobre melhorias",
                    "Considere criar programas de mentoria com usuários experientes",
                    "Analise padrões de uso destes usuários para otimizar a experiência"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top users for module {ModuleName} in application {ApplicationId}",
                moduleName, applicationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
