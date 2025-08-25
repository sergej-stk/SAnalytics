using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Storage;

namespace SAnalytics.Desktop.Services;

public class LocalizationService : ILocalizationService
{
    private ResourceLoader _resourceLoader;
    private CultureInfo _currentCulture;

    public event EventHandler<CultureInfo>? LanguageChanged;

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (!Equals(_currentCulture, value))
            {
                _currentCulture = value;
                LanguageChanged?.Invoke(this, value);
            }
        }
    }

    public LocalizationService()
    {
        _currentCulture = LoadSavedLanguage();
        ApplyCulture(_currentCulture);
        _resourceLoader = new ResourceLoader();
    }

    public string GetString(string key)
    {
        try
        {
            // Create a new ResourceLoader to get the updated language resources
            var loader = new ResourceLoader();
            var value = loader.GetString(key);
            return string.IsNullOrEmpty(value) ? key : value;
        }
        catch
        {
            return key;
        }
    }

    public void SetLanguage(CultureInfo culture)
    {
        if (culture == null)
            return;

        ApplyCulture(culture);
        SaveLanguage(culture);
        CurrentCulture = culture;
        
        // Force reload of ResourceLoader
        _resourceLoader = new ResourceLoader();
    }

    public void SetLanguage(string cultureName)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            SetLanguage(culture);
        }
        catch (CultureNotFoundException)
        {
            // Fallback to English if culture not found
            SetLanguage(CultureInfo.GetCultureInfo("en-US"));
        }
    }

    private void ApplyCulture(CultureInfo culture)
    {
        try
        {
            // Set the primary language override for WinUI 3
            ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
        }
        catch
        {
            // Fallback silently if setting fails
        }
    }

    private CultureInfo LoadSavedLanguage()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue("AppLanguage", out var savedLanguage) && savedLanguage is string langCode)
            {
                return CultureInfo.GetCultureInfo(langCode);
            }
        }
        catch
        {
            // Ignore errors and use default
        }

        // Default to system language or English
        return CultureInfo.CurrentUICulture;
    }

    private void SaveLanguage(CultureInfo culture)
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["AppLanguage"] = culture.Name;
        }
        catch
        {
            // Ignore save errors
        }
    }
}