using Microsoft.Extensions.DependencyInjection;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Auth;
using SAnalytics.Desktop.ViewModels.Settings;

namespace SAnalytics.Desktop.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SettingsViewModel>();
        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ILocalizationService, LocalizationService>();
        return services;
    }
}