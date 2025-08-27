using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Models.Data;
using System;

namespace SAnalytics.Desktop.Services;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    event EventHandler<AppTheme> ThemeChanged;
    void SetTheme(AppTheme theme);
    void Initialize();
}

public class ThemeService : IThemeService
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.System;
    
    public event EventHandler<AppTheme>? ThemeChanged;

    public void Initialize()
    {
        // Load saved theme from settings (default to System)
        CurrentTheme = AppTheme.System;
        ApplyTheme(CurrentTheme);
    }

    public void SetTheme(AppTheme theme)
    {
        if (CurrentTheme != theme)
        {
            CurrentTheme = theme;
            ApplyTheme(theme);
            ThemeChanged?.Invoke(this, theme);
        }
    }

    private void ApplyTheme(AppTheme theme)
    {
        if (Application.Current?.RequestedTheme != null)
        {
            var requestedTheme = theme switch
            {
                AppTheme.Light => ApplicationTheme.Light,
                AppTheme.Dark => ApplicationTheme.Dark,
                AppTheme.System => GetSystemTheme(),
                _ => ApplicationTheme.Light
            };

            // Apply to current window if available
            if (((App)Application.Current).MainWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = requestedTheme == ApplicationTheme.Light 
                    ? ElementTheme.Light 
                    : ElementTheme.Dark;
            }
        }
    }

    private ApplicationTheme GetSystemTheme()
    {
        // Get system theme preference
        var uiSettings = new Windows.UI.ViewManagement.UISettings();
        var systemUsesLightTheme = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString() == "#FFFFFFFF";
        return systemUsesLightTheme ? ApplicationTheme.Light : ApplicationTheme.Dark;
    }
}