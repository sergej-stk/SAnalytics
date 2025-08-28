using SAnalytics.Desktop.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Core.Exceptions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Shows an exception dialog for manual exception handling
    /// </summary>
    /// <param name="exception">The exception to display</param>
    /// <param name="userMessage">Optional user-friendly message</param>
    /// <param name="context">Optional context information for logging</param>
    public static async Task ShowExceptionDialogAsync(this Exception exception, string? userMessage = null, string context = "")
    {
        // Log the exception first
        ExceptionLogger.LogException(exception, "Manual", context);
        
        // Show the dialog
        await WinUI3ExceptionHandler.ShowExceptionDialogAsync(exception, userMessage);
    }
    
    /// <summary>
    /// Logs an exception without showing a dialog
    /// </summary>
    /// <param name="exception">The exception to log</param>
    /// <param name="source">Source of the exception</param>
    /// <param name="context">Optional context information</param>
    public static void LogException(this Exception exception, string source = "Unknown", string context = "")
    {
        ExceptionLogger.LogException(exception, source, context);
    }
}

/// <summary>
/// Helper class for common exception handling patterns in ViewModels and Services
/// </summary>
public static class ExceptionHelper
{
    /// <summary>
    /// Executes an action with automatic exception handling
    /// </summary>
    public static async Task ExecuteWithExceptionHandlingAsync(Func<Task> action, string operation = "Operation")
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await ex.ShowExceptionDialogAsync($"Fehler beim Ausführen von: {operation}", operation);
        }
    }
    
    /// <summary>
    /// Executes a function with automatic exception handling
    /// </summary>
    public static async Task<T?> ExecuteWithExceptionHandlingAsync<T>(Func<Task<T>> function, string operation = "Operation")
    {
        try
        {
            return await function();
        }
        catch (Exception ex)
        {
            await ex.ShowExceptionDialogAsync($"Fehler beim Ausführen von: {operation}", operation);
            return default;
        }
    }
    
    /// <summary>
    /// Executes a synchronous action with automatic exception handling
    /// </summary>
    public static async Task ExecuteWithExceptionHandlingAsync(Action action, string operation = "Operation")
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            await ex.ShowExceptionDialogAsync($"Fehler beim Ausführen von: {operation}", operation);
        }
    }
    
    /// <summary>
    /// Executes a synchronous function with automatic exception handling
    /// </summary>
    public static async Task<T?> ExecuteWithExceptionHandlingAsync<T>(Func<T> function, string operation = "Operation")
    {
        try
        {
            return function();
        }
        catch (Exception ex)
        {
            await ex.ShowExceptionDialogAsync($"Fehler beim Ausführen von: {operation}", operation);
            return default;
        }
    }
}