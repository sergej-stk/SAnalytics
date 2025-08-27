using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAnalytics.Desktop.Core.ViewModels;
using SAnalytics.Desktop.Models.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace SAnalytics.Desktop.ViewModels.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private Language _selectedLanguage;

    [ObservableProperty]
    private string _languageLabel = string.Empty;

    [ObservableProperty]
    private string _additionalSettingsLabel = string.Empty;

    [ObservableProperty]
    private string _additionalOptionsLabel = string.Empty;
    
    [ObservableProperty]
    private string _themeLabel = string.Empty;

    public ObservableCollection<Language> AvailableLanguages { get; }

    public SettingsViewModel()
    {
        AvailableLanguages = new ObservableCollection<Language>(Language.SupportedLanguages);
        _selectedLanguage = Language.English; // Initialize with default first
        UpdateLocalizedStrings();
        _selectedLanguage = GetCurrentLanguage(); // Then update with current
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        UpdateLocalizedStrings();
        if (AvailableLanguages != null && AvailableLanguages.Any())
        {
            SelectedLanguage = GetCurrentLanguage();
        }
    }

    private void UpdateLocalizedStrings()
    {
        Title = GetLocalizedString("Settings");
        LanguageLabel = GetLocalizedString("Language");
        ThemeLabel = GetLocalizedString("Theme");
        AdditionalSettingsLabel = GetLocalizedString("AdditionalSettings");
        AdditionalOptionsLabel = GetLocalizedString("AdditionalOptions");
    }

    private Language GetCurrentLanguage()
    {
        if (AvailableLanguages == null || !AvailableLanguages.Any())
            return Language.English;
            
        var currentCulture = _localizationService.CurrentCulture.Name;
        return AvailableLanguages.FirstOrDefault(l => l.Code == currentCulture) ?? Language.English;
    }

    partial void OnSelectedLanguageChanged(Language value)
    {
        if (value != null && value.Code != _localizationService.CurrentCulture.Name)
        {
            _localizationService.SetLanguage(value.Code);
        }
    }
}