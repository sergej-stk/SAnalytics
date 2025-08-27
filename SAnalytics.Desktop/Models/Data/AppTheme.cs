namespace SAnalytics.Desktop.Models.Data;

public enum AppTheme
{
    System,
    Light,
    Dark
}

public record ThemeOption(AppTheme Theme, string Name, string LocalizedKey)
{
    public static readonly ThemeOption System = new(AppTheme.System, "System", "ThemeSystem");
    public static readonly ThemeOption Light = new(AppTheme.Light, "Light", "ThemeLight");
    public static readonly ThemeOption Dark = new(AppTheme.Dark, "Dark", "ThemeDark");

    public static readonly ThemeOption[] AvailableThemes = 
    {
        System,
        Light,
        Dark
    };
}