using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ModuleHeatMap.SDK.Models;

namespace ModuleHeatMap.SDK.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona o ModuleHeatMap SDK aos serviços
    /// </summary>
    public static IServiceCollection AddModuleHeatMap(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ModuleHeatMapOptions();
        configuration.GetSection("ModuleHeatMap").Bind(options);

        return services.AddModuleHeatMap(options);
    }

    /// <summary>
    /// Adiciona o ModuleHeatMap SDK aos serviços com opções específicas
    /// </summary>
    public static IServiceCollection AddModuleHeatMap(this IServiceCollection services, ModuleHeatMapOptions options)
    {
        services.AddSingleton(options);

        services.AddHttpClient<ModuleHeatMapClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.Add("X-Application-Id", options.ApplicationId);
        });

        return services;
    }

    /// <summary>
    /// Adiciona o ModuleHeatMap SDK com configuração via delegate
    /// </summary>
    public static IServiceCollection AddModuleHeatMap(this IServiceCollection services, Action<ModuleHeatMapOptions> configure)
    {
        var options = new ModuleHeatMapOptions();
        configure(options);

        return services.AddModuleHeatMap(options);
    }
}
