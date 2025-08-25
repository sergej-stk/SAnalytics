namespace SAnalytics.Desktop.Models.Data;

public record Language(string Code, string Name, string NativeName)
{
    public static readonly Language English = new("en-US", "English", "English");
    public static readonly Language German = new("de-DE", "German", "Deutsch");
    public static readonly Language Russian = new("ru-RU", "Russian", "Русский");
    public static readonly Language French = new("fr-FR", "French", "Français");

    public static readonly Language[] SupportedLanguages = 
    {
        English,
        German,
        Russian,
        French
    };
}