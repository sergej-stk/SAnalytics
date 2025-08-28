using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Auth;
using SAnalytics.Desktop.ViewModels.Settings;
using SAnalytics.Desktop.ViewModels.Controls;
using SAnalytics.Desktop.ViewModels.Dialogs;
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
        services.AddTransient<SAnalytics.Desktop.Views.Pages.LoginPage>();
        services.AddTransient<SAnalytics.Desktop.Views.Pages.DashboardPage>();
        services.AddTransient<SAnalytics.Desktop.Views.Pages.SettingsPage>();
        
        // Register Controls
        services.AddTransient<SAnalytics.Desktop.Views.Controls.ThemeSelector>();
        services.AddTransient<SAnalytics.Desktop.Views.Controls.LanguageSelector>();
        services.AddTransient<SAnalytics.Desktop.Views.Controls.HoverThemeSelector>();
        services.AddTransient<SAnalytics.Desktop.Views.Controls.HoverLanguageSelector>();
        
        // Factory for ExceptionDialogViewModel since it needs constructor parameters
        services.AddTransient<Func<Exception, string?, ExceptionDialogViewModel>>(provider =>
        {
            var localizationService = provider.GetRequiredService<ILocalizationService>();
            var logger = provider.GetRequiredService<ILogger<ExceptionDialogViewModel>>();
            return (exception, userMessage) => new ExceptionDialogViewModel(exception, userMessage, localizationService, logger);
        });
        
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
    /// Adds logging services with structured logging configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            
            // Add console logging for development
#if DEBUG
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
#else
            builder.SetMinimumLevel(LogLevel.Information);
#endif
            
            // Add debug output for development
            builder.AddDebug();
            
            // Configure log filtering
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
            builder.AddFilter("SAnalytics", LogLevel.Information);
        });
        
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
            .AddApplicationLogging()
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