using Microsoft.Extensions.DependencyInjection;
using WarriorExperiment.Core.Services;

namespace WarriorExperiment.Core.Extensions;

/// <summary>
/// Extension methods for registering Core services in the DI container
/// </summary>
public static class WaServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Core services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWaCoreServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<WaEntryService>();
        services.AddScoped<WaImageService>();
        
        // Register CRUD services
        services.AddScoped<WaDailySurveyEntryService>();
        services.AddScoped<WaMeasurementEntryService>();
        services.AddScoped<WaRiteOfPassagePracticeEntryService>();
        services.AddScoped<WaVarietyPracticeEntryService>();
        services.AddScoped<WaUserService>();
        
        return services;
    }
}