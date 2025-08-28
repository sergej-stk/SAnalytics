using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using SAnalytics.Desktop.Services;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Core.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Provides common functionality including localization, logging, and basic MVVM support.
/// Uses proper dependency injection instead of service locator pattern.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject, IDisposable
{
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private bool _disposed;

    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private bool _hasErrors;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the BaseViewModel class with proper dependency injection.
    /// </summary>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="logger">The logger instance.</param>
    protected BaseViewModel(
        ILocalizationService localizationService, 
        ILogger logger)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Initialize localized strings
        OnLanguageChanged(_localizationService, _localizationService.CurrentCulture);
        
        _logger.LogDebug("ViewModel {ViewModelType} initialized", GetType().Name);
    }

    /// <summary>
    /// Called when the application language changes.
    /// Override in derived classes to update localized properties.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="culture">The new culture.</param>
    protected virtual void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        // Override in derived classes to update localized properties
        _logger.LogDebug("Language changed to {Culture} for {ViewModelType}", culture.Name, GetType().Name);
    }

    /// <summary>
    /// Gets a localized string for the specified key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <returns>The localized string.</returns>
    protected string GetLocalizedString(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Attempted to get localized string with null or empty key");
            return string.Empty;
        }
        
        return _localizationService.GetString(key);
    }

    /// <summary>
    /// Sets the busy state and optionally displays a message.
    /// </summary>
    /// <param name="isBusy">Whether the ViewModel is busy.</param>
    /// <param name="message">Optional message to display.</param>
    protected virtual void SetBusyState(bool isBusy, string? message = null)
    {
        IsBusy = isBusy;
        
        if (!string.IsNullOrEmpty(message))
        {
            _logger.LogInformation("ViewModel {ViewModelType} busy state: {IsBusy}, Message: {Message}", 
                GetType().Name, isBusy, message);
        }
    }

    /// <summary>
    /// Sets an error state with a message.
    /// </summary>
    /// <param name="errorMessage">The error message to display.</param>
    /// <param name="logError">Whether to log the error.</param>
    protected virtual void SetError(string errorMessage, bool logError = true)
    {
        HasErrors = !string.IsNullOrEmpty(errorMessage);
        ErrorMessage = errorMessage ?? string.Empty;
        
        if (logError && HasErrors)
        {
            _logger.LogError("ViewModel {ViewModelType} error: {ErrorMessage}", GetType().Name, errorMessage);
        }
    }

    /// <summary>
    /// Clears any error state.
    /// </summary>
    protected virtual void ClearError()
    {
        HasErrors = false;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Executes an async operation with automatic busy state management and error handling.
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="busyMessage">Optional message to display while busy.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the operation completed successfully, false otherwise.</returns>
    protected async Task<bool> ExecuteWithBusyStateAsync(
        Func<CancellationToken, Task> operation,
        string? busyMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        try
        {
            SetBusyState(true, busyMessage);
            ClearError();
            
            await operation(cancellationToken);
            
            _logger.LogDebug("Async operation completed successfully in {ViewModelType}", GetType().Name);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation cancelled in {ViewModelType}", GetType().Name);
            SetError(GetLocalizedString("OperationCancelled"));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing async operation in {ViewModelType}", GetType().Name);
            SetError(GetLocalizedString("UnexpectedError"));
            return false;
        }
        finally
        {
            SetBusyState(false);
        }
    }

    /// <summary>
    /// Executes an async operation with automatic busy state management and error handling.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="busyMessage">Optional message to display while busy.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result of the operation, or default(T) if an error occurred.</returns>
    protected async Task<T?> ExecuteWithBusyStateAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string? busyMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        try
        {
            SetBusyState(true, busyMessage);
            ClearError();
            
            var result = await operation(cancellationToken);
            
            _logger.LogDebug("Async operation completed successfully in {ViewModelType}", GetType().Name);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation cancelled in {ViewModelType}", GetType().Name);
            SetError(GetLocalizedString("OperationCancelled"));
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing async operation in {ViewModelType}", GetType().Name);
            SetError(GetLocalizedString("UnexpectedError"));
            return default(T);
        }
        finally
        {
            SetBusyState(false);
        }
    }

    /// <summary>
    /// Gets the logger instance for this ViewModel.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Gets the localization service instance.
    /// </summary>
    protected ILocalizationService LocalizationService => _localizationService;

    /// <summary>
    /// Releases all resources used by the BaseViewModel.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases all resources used by the BaseViewModel.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Unsubscribe from events to prevent memory leaks
            if (_localizationService != null)
            {
                _localizationService.LanguageChanged -= OnLanguageChanged;
            }
            
            _logger.LogDebug("ViewModel {ViewModelType} disposed", GetType().Name);
            _disposed = true;
        }
    }
}