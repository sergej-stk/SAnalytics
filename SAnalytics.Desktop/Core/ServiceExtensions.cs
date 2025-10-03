using Microsoft.Extensions.DependencyInjection;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Auth;
using SAnalytics.Desktop.ViewModels.Settings;
using SAnalytics.Desktop.ViewModels.Controls;

using System;

namespace SAnalytics.Desktop.Core;

/// <summary>
/// Extension methods for configuring application services in the dependency injection container.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds all ViewModels to the service collection with proper lifetime management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // ViewModels should be transient as they represent UI state
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ThemeSelectorViewModel>();
        
        // Register Pages
        services.AddTransient<Views.Pages.LoginPage>();
        services.AddTransient<Views.Pages.DashboardPage>();
        services.AddTransient<Views.Pages.SettingsPage>();
        
        // Register Controls
        services.AddTransient<Views.Controls.ThemeSelector>();
        services.AddTransient<Views.Controls.LanguageSelector>();
        services.AddTransient<Views.Controls.HoverThemeSelector>();
        services.AddTransient<Views.Controls.HoverLanguageSelector>();
        
        
        
        return services;
    }
    
    /// <summary>
    /// Adds core application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Configuration service - singleton for application lifetime
        services.AddSingleton<IAppConfigurationService, AppConfigurationService>();
        
        // Navigation service - singleton for application lifetime
        services.AddSingleton<INavigationService, NavigationService>();
        
        // Authentication service - singleton for session management
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        
        // Localization service - singleton for application lifetime
        services.AddSingleton<ILocalizationService, LocalizationService>();
        
        // Theme service - singleton for application lifetime
        services.AddSingleton<IThemeService, ThemeService>();
        
        return services;
    } 
    
    /// <summary>
    /// Configures application-wide options and settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureApplicationOptions(this IServiceCollection services)
    {
        // Configure any application-wide options here
        // Example: services.Configure<ApplicationOptions>(options => { });
        
        return services;
    }
    
    /// <summary>
    /// Adds all application services in the correct order.
    /// This is the main method to call for complete service configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services
            .AddCoreServices()
            .AddViewModels()
            .ConfigureApplicationOptions();
    }
    
    /// <summary>
    /// Validates that all required services are properly registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required services are missing.</exception>
    public static IServiceCollection ValidateServices(this IServiceCollection services)
    {
        // Build a temporary service provider to validate registrations
        using var serviceProvider = services.BuildServiceProvider();
        
        // Validate critical services are registered
        var criticalServices = new[]
        {
            typeof(INavigationService),
            typeof(IAuthenticationService),
            typeof(IAppConfigurationService),
            typeof(ILocalizationService),
            typeof(IThemeService)
        };
        
        foreach (var serviceType in criticalServices)
        {
            var service = serviceProvider.GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException($"Critical service {serviceType.Name} is not registered");
            }
        }
        
        return services;
    }
}