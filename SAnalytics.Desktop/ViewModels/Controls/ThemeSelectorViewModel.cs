using CommunityToolkit.Mvvm.ComponentModel;
using SAnalytics.Desktop.Core.ViewModels;
using SAnalytics.Desktop.Models.Data;
using SAnalytics.Desktop.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SAnalytics.Desktop.ViewModels.Controls;

public partial class ThemeOptionViewModel : BaseViewModel
{
    public AppTheme Theme { get; }
    public string LocalizedKey { get; }
    
    [ObservableProperty]
    private string _localizedName = string.Empty;

    public ThemeOptionViewModel(AppTheme theme, string localizedKey)
    {
        Theme = theme;
        LocalizedKey = localizedKey;
        UpdateLocalizedName();
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        UpdateLocalizedName();
    }

    private void UpdateLocalizedName()
    {
        LocalizedName = GetLocalizedString(LocalizedKey);
    }
}

public partial class ThemeSelectorViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private ThemeOptionViewModel? _selectedTheme;
    
    [ObservableProperty]
    private string _themeLabel = string.Empty;

    public ObservableCollection<ThemeOptionViewModel> AvailableThemes { get; }

    public ThemeSelectorViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        
        AvailableThemes = new ObservableCollection<ThemeOptionViewModel>(
            ThemeOption.AvailableThemes.Select(theme => 
                new ThemeOptionViewModel(theme.Theme, theme.LocalizedKey)));

        _selectedTheme = AvailableThemes.FirstOrDefault(t => t.Theme == _themeService.CurrentTheme);
        
        _themeService.ThemeChanged += OnThemeChanged;
        UpdateThemeLabel();
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        UpdateThemeLabel();
        // ThemeOptionViewModel handles its own localization updates
    }

    private void OnThemeChanged(object? sender, AppTheme newTheme)
    {
        SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Theme == newTheme);
    }

    partial void OnSelectedThemeChanged(ThemeOptionViewModel? value)
    {
        if (value != null && value.Theme != _themeService.CurrentTheme)
        {
            _themeService.SetTheme(value.Theme);
        }
    }
    
    private void UpdateThemeLabel()
    {
        ThemeLabel = GetLocalizedString("Theme");
    }
}