using Microsoft.Extensions.DependencyInjection;
using WarriorExperiment.Core.Interfaces;
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
        services.AddTransient<WaEntryService>();
        services.AddTransient<WaImageService>();
        
        // Register CRUD services
        services.AddTransient<WaDailySurveyEntryService>();
        services.AddTransient<WaMeasurementEntryService>();
        services.AddTransient<WaRiteOfPassagePracticeEntryService>();
        services.AddTransient<WaVarietyPracticeEntryService>();
        services.AddTransient<WaUserService>();
        
        // Register demo data service
        services.AddTransient<WaDemoDataService>();
        
        // Register backup and restore services
        services.AddTransient<IWaBackupService, WaBackupService>();
        services.AddTransient<IWaUserDataService, WaUserDataService>();
        
        return services;
    }
}