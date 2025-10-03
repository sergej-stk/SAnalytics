using Microsoft.Extensions.Logging;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Globalization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Storage;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Implementation of localization service using WinUI 3 ResourceLoader.
/// Provides multilingual support with persistent language preferences.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private ResourceLoader _resourceLoader;
    private CultureInfo _currentCulture;
    
    // Supported cultures in the application
    private static readonly CultureInfo[] SupportedCultures = 
    {
        new("en-US"), // English
        new("de-DE"), // German  
        new("fr-FR"), // French
        new("ru-RU")  // Russian
    };

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

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _currentCulture = LoadSavedLanguage();
        ApplyCulture(_currentCulture);
        _resourceLoader = new ResourceLoader();
        
        Log.Information("LocalizationService initialized with culture {Culture}", _currentCulture.Name);
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Log.Warning("Attempted to get localized string with null or empty key");
            return string.Empty;
        }

        try
        {
            // Create a new ResourceLoader to get the updated language resources
            var loader = new ResourceLoader();
            var value = loader.GetString(key);
            return string.IsNullOrEmpty(value) ? key : value;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get localized string for key '{Key}'", key);
            return key;
        }
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        
        if (args == null || args.Length == 0)
            return format;

        try
        {
            return string.Format(format, args);
        }
        catch (FormatException ex)
        {
            Log.Warning(ex, "Failed to format localized string for key '{Key}' with {ArgCount} arguments", key, args.Length);
            return format;
        }
    }

    public bool SetCulture(CultureInfo culture)
    {
        if (culture == null)
        {
            Log.Warning("Attempted to set culture with null value");
            return false;
        }

        try
        {
            // Check if culture is supported
            if (!SupportedCultures.Any(c => c.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Warning("Culture {Culture} is not supported, falling back to English", culture.Name);
                culture = SupportedCultures.First(c => c.Name == "en-US");
            }

            ApplyCulture(culture);
            SaveLanguage(culture);
            CurrentCulture = culture;
            
            // Force reload of ResourceLoader
            _resourceLoader = new ResourceLoader();
            
            Log.Information("Culture changed to {Culture}", culture.Name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to set culture to {Culture}", culture.Name);
            return false;
        }
    }

    public bool SetCulture(string cultureCode)
    {
        if (string.IsNullOrEmpty(cultureCode))
        {
            Log.Warning("Attempted to set culture with null or empty culture code");
            return false;
        }

        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureCode);
            return SetCulture(culture);
        }
        catch (CultureNotFoundException ex)
        {
            Log.Warning(ex, "Culture '{CultureCode}' not found, falling back to English", cultureCode);
            return SetCulture(SupportedCultures.First(c => c.Name == "en-US"));
        }
    }

    public CultureInfo[] GetAvailableCultures()
    {
        return SupportedCultures.ToArray();
    }

    public void SetLanguage(CultureInfo culture)
    {
        SetCulture(culture);
    }

    public void SetLanguage(string cultureName)
    {
        SetCulture(cultureName);
    }

    private void ApplyCulture(CultureInfo culture)
    {
        try
        {
            // Set the primary language override for WinUI 3
            ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
            Log.Debug("Applied culture {Culture} to ApplicationLanguages", culture.Name);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to apply culture {Culture}", culture.Name);
        }
    }

    private CultureInfo LoadSavedLanguage()
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue("AppLanguage", out var savedLanguage) && savedLanguage is string langCode)
            {
                var culture = CultureInfo.GetCultureInfo(langCode);
                Log.Debug("Loaded saved language {Culture}", culture.Name);
                return culture;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load saved language, using default");
        }

        // Default to system language or English if not supported
        var defaultCulture = CultureInfo.CurrentUICulture;
        if (!SupportedCultures.Any(c => c.Name.Equals(defaultCulture.Name, StringComparison.OrdinalIgnoreCase)))
        {
            defaultCulture = SupportedCultures.First(c => c.Name == "en-US");
        }
        
        Log.Debug("Using default culture {Culture}", defaultCulture.Name);
        return defaultCulture;
    }

    private void SaveLanguage(CultureInfo culture)
    {
        try
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["AppLanguage"] = culture.Name;
            Log.Debug("Saved language preference {Culture}", culture.Name);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save language preference {Culture}", culture.Name);
        }
    }
}