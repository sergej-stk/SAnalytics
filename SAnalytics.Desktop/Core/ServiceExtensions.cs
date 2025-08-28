using Microsoft.Extensions.DependencyInjection;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Auth;
using SAnalytics.Desktop.ViewModels.Settings;
using SAnalytics.Desktop.ViewModels.Controls;
using SAnalytics.Desktop.ViewModels.Debug;

namespace SAnalytics.Desktop.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ThemeSelectorViewModel>();
#if DEBUG
        services.AddTransient<ExceptionTestViewModel>();
#endif
        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // services.AddSingleton<IAppSettingsService, AppSettingsService>(); // Commented out for now
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        return services;
    }
}