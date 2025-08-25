using CommunityToolkit.Mvvm.ComponentModel;
using SAnalytics.Desktop.Services;
using System.Globalization;

namespace SAnalytics.Desktop.Core.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string _title = string.Empty;

    protected BaseViewModel()
    {
        _localizationService = App.GetService<ILocalizationService>();
        _localizationService.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged(_localizationService, _localizationService.CurrentCulture);
    }

    protected virtual void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        // Override in derived classes to update localized properties
    }

    protected string GetLocalizedString(string key)
    {
        return _localizationService.GetString(key);
    }
}