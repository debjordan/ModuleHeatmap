using Microsoft.AspNetCore.Mvc;
using ModuleHeatMap.Application.DTOs;
using ModuleHeatMap.Core.Interfaces;
using FluentValidation;

namespace ModuleHeatMap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly IModuleHeatMapService _heatMapService;
    private readonly IValidator<ModuleAccessDto> _validator;
    private readonly ILogger<TrackingController> _logger;

    public TrackingController(
        IModuleHeatMapService heatMapService,
        IValidator<ModuleAccessDto> validator,
        ILogger<TrackingController> logger)
    {
        _heatMapService = heatMapService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Registra um acesso a um módulo
    /// </summary>
    [HttpPost("track")]
    public async Task<IActionResult> TrackAccess([FromBody] ModuleAccessDto request)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await _heatMapService.TrackModuleAccessAsync(
                request.ApplicationId,
                request.UserId,
                request.ModuleName,
                request.ModuleUrl,
                request.AccessType,
                request.Metadata
            );

            return Ok(new { success = true, message = "Access tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking access for module {ModuleName}", request.ModuleName);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Registra múltiplos acessos em batch
    /// </summary>
    [HttpPost("track/batch")]
    public async Task<IActionResult> TrackBatchAccess([FromBody] IEnumerable<ModuleAccessDto> requests)
    {
        try
        {
            var results = new List<object>();

            foreach (var request in requests)
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    results.Add(new {
                        moduleName = request.ModuleName,
                        success = false,
                        errors = validationResult.Errors.Select(e => e.ErrorMessage)
                    });
                    continue;
                }

                try
                {
                    await _heatMapService.TrackModuleAccessAsync(
                        request.ApplicationId,
                        request.UserId,
                        request.ModuleName,
                        request.ModuleUrl,
                        request.AccessType,
                        request.Metadata
                    );

                    results.Add(new {
                        moduleName = request.ModuleName,
                        success = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error tracking batch access for module {ModuleName}", request.ModuleName);
                    results.Add(new {
                        moduleName = request.ModuleName,
                        success = false,
                        error = "Processing error"
                    });
                }
            }

            return Ok(new { results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch tracking request");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}
