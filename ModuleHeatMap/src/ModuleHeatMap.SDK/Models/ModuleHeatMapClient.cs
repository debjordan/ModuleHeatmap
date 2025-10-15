using System.Text;
using System.Text.Json;
using ModuleHeatMap.SDK.Models;
using Microsoft.Extensions.Logging;

namespace ModuleHeatMap.SDK;

public class ModuleHeatMapClient
{
    private readonly HttpClient _httpClient;
    private readonly ModuleHeatMapOptions _options;
    private readonly ILogger<ModuleHeatMapClient>? _logger;

    public ModuleHeatMapClient(HttpClient httpClient, ModuleHeatMapOptions options, ILogger<ModuleHeatMapClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Application-Id", options.ApplicationId);
    }

    /// <summary>
    /// Rastreia um acesso a módulo de forma assíncrona
    /// </summary>
    public async Task<bool> TrackAsync(string userId, string moduleName, string moduleUrl, AccessType accessType, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var request = new TrackingRequest
            {
                ApplicationId = _options.ApplicationId,
                UserId = userId,
                ModuleName = moduleName,
                ModuleUrl = moduleUrl,
                AccessType = accessType,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/tracking/track", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to track module access. Status: {StatusCode}, Module: {ModuleName}",
                    response.StatusCode, moduleName);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error tracking module access for {ModuleName}", moduleName);
            return false;
        }
    }

    /// <summary>
    /// Rastreia múltiplos acessos em lote
    /// </summary>
    public async Task<BatchTrackingResult> TrackBatchAsync(IEnumerable<TrackingRequest> requests)
    {
        try
        {
            var json = JsonSerializer.Serialize(requests, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/tracking/track/batch", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BatchTrackingResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                return result ?? new BatchTrackingResult();
            }

            return new BatchTrackingResult();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in batch tracking");
            return new BatchTrackingResult();
        }
    }

    /// <summary>
    /// Rastreia um acesso de forma fire-and-forget (sem aguardar resposta)
    /// </summary>
    public void TrackFireAndForget(string userId, string moduleName, string moduleUrl, AccessType accessType, Dictionary<string, object>? metadata = null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await TrackAsync(userId, moduleName, moduleUrl, accessType, metadata);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in fire-and-forget tracking for {ModuleName}", moduleName);
            }
        });
    }

    /// <summary>
    /// Obtém o heatmap da aplicação
    /// </summary>
    public async Task<HeatMapResponse?> GetHeatMapAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryString = BuildQueryString(new Dictionary<string, object?>
            {
                ["startDate"] = startDate?.ToString("yyyy-MM-dd"),
                ["endDate"] = endDate?.ToString("yyyy-MM-dd")
            });

            var response = await _httpClient.GetAsync($"/api/analytics/{_options.ApplicationId}/heatmap{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<HeatMapResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting heatmap");
            return null;
        }
    }

    /// <summary>
    /// Obtém analytics de um módulo específico
    /// </summary>
    public async Task<ModuleAnalyticsResponse?> GetModuleAnalyticsAsync(string moduleName, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryString = BuildQueryString(new Dictionary<string, object?>
            {
                ["startDate"] = startDate?.ToString("yyyy-MM-dd"),
                ["endDate"] = endDate?.ToString("yyyy-MM-dd")
            });

            var response = await _httpClient.GetAsync($"/api/analytics/{_options.ApplicationId}/modules/{moduleName}/analytics{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ModuleAnalyticsResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting module analytics for {ModuleName}", moduleName);
            return null;
        }
    }

    private static string BuildQueryString(Dictionary<string, object?> parameters)
    {
        var validParams = parameters.Where(p => p.Value != null);
        if (!validParams.Any()) return "";

        var queryParams = validParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!.ToString()!)}");
        return "?" + string.Join("&", queryParams);
    }
}
