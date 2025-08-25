using System;
using System.Globalization;

namespace SAnalytics.Desktop.Services;

public interface ILocalizationService
{
    event EventHandler<CultureInfo>? LanguageChanged;
    CultureInfo CurrentCulture { get; }
    string GetString(string key);
    void SetLanguage(CultureInfo culture);
    void SetLanguage(string cultureName);
}