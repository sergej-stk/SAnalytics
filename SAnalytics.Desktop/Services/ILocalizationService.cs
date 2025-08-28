using System;
using System.Globalization;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Service for handling application localization and language management.
/// Provides access to localized strings and culture management.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current culture being used by the application.
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets a localized string for the specified key.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string with format parameters.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="args">The format arguments.</param>
    /// <returns>The formatted localized string.</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Changes the application language.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    /// <returns>True if the culture was changed successfully.</returns>
    bool SetCulture(CultureInfo culture);

    /// <summary>
    /// Changes the application language by culture code.
    /// </summary>
    /// <param name="cultureCode">The culture code (e.g., "en-US", "de-DE").</param>
    /// <returns>True if the culture was changed successfully.</returns>
    bool SetCulture(string cultureCode);

    /// <summary>
    /// Gets the available cultures/languages in the application.
    /// </summary>
    /// <returns>An array of supported cultures.</returns>
    CultureInfo[] GetAvailableCultures();

    /// <summary>
    /// Event raised when the application language changes.
    /// </summary>
    event EventHandler<CultureInfo>? LanguageChanged;

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    void SetLanguage(CultureInfo culture);

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    void SetLanguage(string cultureName);
}