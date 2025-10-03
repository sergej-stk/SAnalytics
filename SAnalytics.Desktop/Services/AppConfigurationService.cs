using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Implementation of application configuration service using Windows.Storage.ApplicationData.
/// Provides persistent configuration storage for WinUI 3 applications.
/// </summary>
public class AppConfigurationService : IAppConfigurationService
{
    private readonly ConcurrentDictionary<string, object?> _configCache;
    private readonly SemaphoreSlim _semaphore;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private static readonly string ConfigFileName = "appsettings.json";
    
    public string AppVersion { get; private set; } = string.Empty;
    public string AppName { get; private set; } = string.Empty;
    public bool IsDebugMode { get; private set; }
    
    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public AppConfigurationService()
    {
        _configCache = new ConcurrentDictionary<string, object?>();
        _semaphore = new SemaphoreSlim(1, 1);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true
        };
        
        InitializeAppInfo();
    }

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        if (string.IsNullOrEmpty(key))
        {
            Log.Warning("Attempted to get configuration value with null or empty key");
            return defaultValue;
        }

        try
        {
            if (_configCache.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue is T typedValue)
                    return typedValue;
                
                if (cachedValue is JsonElement jsonElement)
                    return DeserializeJsonElement<T>(jsonElement, defaultValue);
                
                // Attempt type conversion
                if (cachedValue != null && typeof(T).IsAssignableFrom(cachedValue.GetType()))
                    return (T)cachedValue;
                
                return ConvertValue<T>(cachedValue) ?? defaultValue;
            }

            Log.Debug("Configuration key '{Key}' not found, returning default value", key);
            return defaultValue;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting configuration value for key '{Key}'", key);
            return defaultValue;
        }
    }

    public async Task<T> GetValueAsync<T>(string key, T defaultValue = default!)
    {
        if (string.IsNullOrEmpty(key))
        {
            Log.Warning("Attempted to get configuration value with null or empty key");
            return defaultValue;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Try cache first
            if (_configCache.TryGetValue(key, out var cachedValue))
            {
                return ConvertCachedValue<T>(cachedValue, defaultValue);
            }

            // Load from storage if not in cache
            await LoadConfigurationAsync();
            
            // Try cache again after loading
            if (_configCache.TryGetValue(key, out cachedValue))
            {
                return ConvertCachedValue<T>(cachedValue, defaultValue);
            }

            Log.Debug("Configuration key '{Key}' not found after loading, returning default value", key);
            return defaultValue;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting configuration value asynchronously for key '{Key}'", key);
            return defaultValue;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Log.Warning("Attempted to set configuration value with null or empty key");
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            var oldValue = _configCache.TryGetValue(key, out var existing) ? existing : null;
            _configCache[key] = value;
            
            await SaveConfigurationAsync();
            
            OnConfigurationChanged(new ConfigurationChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = value
            });
            
            Log.Debug("Configuration value set for key '{Key}'", key);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error setting configuration value for key '{Key}'", key);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool HasKey(string key)
    {
        return !string.IsNullOrEmpty(key) && _configCache.ContainsKey(key);
    }

    public async Task RemoveKeyAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Log.Warning("Attempted to remove configuration key with null or empty key");
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_configCache.TryRemove(key, out var oldValue))
            {
                await SaveConfigurationAsync();
                
                OnConfigurationChanged(new ConfigurationChangedEventArgs
                {
                    Key = key,
                    OldValue = oldValue,
                    NewValue = null
                });
                
                Log.Debug("Configuration key '{Key}' removed", key);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error removing configuration key '{Key}'", key);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ClearAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _configCache.Clear();
            await SaveConfigurationAsync();
            
            Log.Information("All configuration values cleared");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error clearing all configuration values");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ReloadAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _configCache.Clear();
            await LoadConfigurationAsync();
            
            Log.Information("Configuration reloaded from storage");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reloading configuration");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void InitializeAppInfo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            
            AppName = assemblyName.Name ?? "SAnalytics";
            AppVersion = assemblyName.Version?.ToString() ?? "1.0.0.0";
            
#if DEBUG
            IsDebugMode = true;
#else
            IsDebugMode = false;
#endif
            
            Log.Information("App info initialized: {AppName} v{AppVersion}, Debug: {IsDebug}", 
                AppName, AppVersion, IsDebugMode);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing app info");
            
            AppName = "SAnalytics";
            AppVersion = "1.0.0.0";
            IsDebugMode = false;
        }
    }

    private async Task LoadConfigurationAsync()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var configFile = await localFolder.TryGetItemAsync(ConfigFileName) as StorageFile;
            
            if (configFile == null)
            {
                Log.Information("Configuration file not found, using defaults");
                await InitializeDefaultConfigurationAsync();
                return;
            }

            var jsonContent = await FileIO.ReadTextAsync(configFile);
            
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                Log.Warning("Configuration file is empty, using defaults");
                await InitializeDefaultConfigurationAsync();
                return;
            }

            var configData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent, _jsonOptions);
            
            if (configData != null)
            {
                foreach (var kvp in configData)
                {
                    _configCache[kvp.Key] = kvp.Value;
                }
                
                Log.Information("Configuration loaded successfully with {Count} keys", configData.Count);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading configuration, using defaults");
            await InitializeDefaultConfigurationAsync();
        }
    }

    private async Task SaveConfigurationAsync()
    {
        try
        {
            var configData = new Dictionary<string, object?>();
            
            foreach (var kvp in _configCache)
            {
                configData[kvp.Key] = kvp.Value;
            }

            var jsonContent = JsonSerializer.Serialize(configData, _jsonOptions);
            
            var localFolder = ApplicationData.Current.LocalFolder;
            var configFile = await localFolder.CreateFileAsync(ConfigFileName, CreationCollisionOption.ReplaceExisting);
            
            await FileIO.WriteTextAsync(configFile, jsonContent);
            
            Log.Debug("Configuration saved successfully with {Count} keys", configData.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving configuration");
            throw;
        }
    }

    private async Task InitializeDefaultConfigurationAsync()
    {
        try
        {
            // Set default configuration values
            _configCache[ConfigurationKeys.Language] = "en-US";
            _configCache[ConfigurationKeys.Theme] = "System";
            _configCache[ConfigurationKeys.WindowWidth] = 1200.0;
            _configCache[ConfigurationKeys.WindowHeight] = 800.0;
            _configCache[ConfigurationKeys.WindowMaximized] = false;
            _configCache[ConfigurationKeys.RememberLogin] = false;
            _configCache[ConfigurationKeys.AutoSaveInterval] = TimeSpan.FromMinutes(5).TotalMilliseconds;
            _configCache[ConfigurationKeys.LogLevel] = "Information";
            
            await SaveConfigurationAsync();
            
            Log.Information("Default configuration initialized");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing default configuration");
        }
    }

    private T ConvertCachedValue<T>(object? cachedValue, T defaultValue)
    {
        if (cachedValue is T typedValue)
            return typedValue;
        
        if (cachedValue is JsonElement jsonElement)
            return DeserializeJsonElement<T>(jsonElement, defaultValue);
        
        return ConvertValue<T>(cachedValue) ?? defaultValue;
    }

    private T? ConvertValue<T>(object? value)
    {
        if (value == null)
            return default(T);

        try
        {
            if (value is T directValue)
                return directValue;

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error converting value of type {ValueType} to {TargetType}", 
                value.GetType().Name, typeof(T).Name);
            return default(T);
        }
    }

    private T DeserializeJsonElement<T>(JsonElement element, T defaultValue)
    {
        try
        {
            return element.Deserialize<T>(_jsonOptions) ?? defaultValue;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error deserializing JsonElement to {TargetType}", typeof(T).Name);
            return defaultValue;
        }
    }

    protected virtual void OnConfigurationChanged(ConfigurationChangedEventArgs args)
    {
        ConfigurationChanged?.Invoke(this, args);
    }
}