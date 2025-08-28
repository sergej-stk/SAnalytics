using System;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Service for managing application configuration settings.
/// Provides a centralized way to handle app settings, user preferences, and environment-specific configurations.
/// </summary>
public interface IAppConfigurationService
{
    /// <summary>
    /// Gets a configuration value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <returns>The configuration value or the default value.</returns>
    T GetValue<T>(string key, T defaultValue = default!);
    
    /// <summary>
    /// Gets a configuration value asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <returns>A task containing the configuration value or the default value.</returns>
    Task<T> GetValueAsync<T>(string key, T defaultValue = default!);
    
    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>A task representing the operation.</returns>
    Task SetValueAsync<T>(string key, T value);
    
    /// <summary>
    /// Checks if a configuration key exists.
    /// </summary>
    /// <param name="key">The configuration key to check.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    bool HasKey(string key);
    
    /// <summary>
    /// Removes a configuration key and its value.
    /// </summary>
    /// <param name="key">The configuration key to remove.</param>
    /// <returns>A task representing the operation.</returns>
    Task RemoveKeyAsync(string key);
    
    /// <summary>
    /// Clears all configuration values.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    Task ClearAllAsync();
    
    /// <summary>
    /// Reloads configuration from storage.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    Task ReloadAsync();
    
    /// <summary>
    /// Gets the application version.
    /// </summary>
    string AppVersion { get; }
    
    /// <summary>
    /// Gets the application name.
    /// </summary>
    string AppName { get; }
    
    /// <summary>
    /// Gets whether the application is running in debug mode.
    /// </summary>
    bool IsDebugMode { get; }
    
    /// <summary>
    /// Event raised when a configuration value changes.
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}

/// <summary>
/// Event arguments for configuration change events.
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    public string Key { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Common configuration keys used throughout the application.
/// </summary>
public static class ConfigurationKeys
{
    public const string Language = "App.Language";
    public const string Theme = "App.Theme";
    public const string WindowWidth = "Window.Width";
    public const string WindowHeight = "Window.Height";
    public const string WindowMaximized = "Window.Maximized";
    public const string RememberLogin = "Auth.RememberLogin";
    public const string LastLoginUser = "Auth.LastUser";
    public const string AutoSaveInterval = "App.AutoSaveInterval";
    public const string LogLevel = "Logging.Level";
    public const string DataDirectory = "Data.Directory";
    public const string BackupDirectory = "Backup.Directory";
    public const string ExportFormat = "Export.DefaultFormat";
}