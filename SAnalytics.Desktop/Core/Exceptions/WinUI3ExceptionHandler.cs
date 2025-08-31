using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.ViewModels.Dialogs;
using SAnalytics.Desktop.Views.Dialogs;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Core.Exceptions;

public static class WinUI3ExceptionHandler
{
    private static Exception? _lastFirstChanceException;
    private static ExceptionDialog? _currentDialog;
    private static readonly SemaphoreSlim _dialogSemaphore = new(1, 1);
    
    public static event System.UnhandledExceptionEventHandler? UnhandledException;
    
    static WinUI3ExceptionHandler()
    {
        // Preserve full exception details for WinUI 3 (workaround for stack trace loss)
        AppDomain.CurrentDomain.FirstChanceException += (_, args) =>
        {
            _lastFirstChanceException = args.Exception;
        };
        
        // Background thread exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            if (ex != null)
            {
                var isTerminating = args.IsTerminating || App.IsShuttingDown;
                ExceptionLogger.LogException(ex, "AppDomain", isTerminating ? "Terminating" : "");

                if (isTerminating)
                {
                    ShowSystemErrorMessage($"A fatal error occurred during shutdown: {ex.Message}");
                }
                else
                {
                    ShowExceptionDialogSafe(ex);
                }

                UnhandledException?.Invoke(sender, args);
            }
        };
        
        // Task scheduler exceptions (only fires after GC)
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            ExceptionLogger.LogException(args.Exception, "TaskScheduler");
            args.SetObserved();
            ShowExceptionDialogSafe(args.Exception);
        };
    }
    
    public static void Initialize(Application app)
    {
        // WinUI 3 UI thread exceptions
        app.UnhandledException += (sender, args) =>
        {
            var exception = args.Exception;
            
            // Restore full stack trace if lost (known WinUI 3 issue)
            if (exception.StackTrace is null && 
                _lastFirstChanceException?.Message == exception.Message)
            {
                exception = _lastFirstChanceException;
            }
            
            ExceptionLogger.LogException(exception, "WinUI");
            
            args.Handled = ShouldHandleException(exception);
            
            if (args.Handled)
            {
                ShowExceptionDialogSafe(exception);
            }
        };
    }
    
    private static bool ShouldHandleException(Exception ex)
    {
        // Don't handle fatal exceptions - let app terminate
        return ex is not (OutOfMemoryException or StackOverflowException or AccessViolationException);
    }
    
    private static async void ShowExceptionDialogSafe(Exception ex)
    {
        if (App.IsShuttingDown)
        {
            ExceptionLogger.LogException(ex, "Shutdown");
            ShowSystemErrorMessage($"A non-critical error occurred during shutdown: {ex.Message}");
            return;
        }

        if (!await _dialogSemaphore.WaitAsync(100))
            return; // Prevent multiple dialogs
        
        try
        {
            if (_currentDialog != null) 
                return;
            
            var mainWindow = GetMainWindow();
            if (mainWindow?.Content?.XamlRoot != null)
            {
                var factory = GetExceptionDialogFactory();
                if (factory != null)
                {
                    _currentDialog = new ExceptionDialog(ex, null, factory)
                    {
                        XamlRoot = mainWindow.Content.XamlRoot
                    };
                    await _currentDialog.ShowAsync();
                    _currentDialog = null;
                }
                else
                {
                    // Fallback if DI not available
                    ShowSystemErrorMessage($"Application Error: {ex.Message}");
                }
            }
        }
        catch (Exception dialogEx)
        {
            // Fallback to system message box
            ShowSystemErrorMessage($"Application Error: {ex.Message}");
            ExceptionLogger.LogException(dialogEx, "DialogDisplay");
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }
    
    public static async Task ShowExceptionDialogAsync(Exception ex, string? userMessage = null)
    {
        if (App.IsShuttingDown)
        {
            ExceptionLogger.LogException(ex, "Shutdown");
            ShowSystemErrorMessage(userMessage ?? $"An error occurred during shutdown: {ex.Message}");
            return;
        }

        if (!await _dialogSemaphore.WaitAsync(1000))
            return;
        
        try
        {
            var mainWindow = GetMainWindow();
            if (mainWindow?.Content?.XamlRoot != null)
            {
                var factory = GetExceptionDialogFactory();
                if (factory != null)
                {
                    var dialog = new ExceptionDialog(ex, userMessage, factory)
                    {
                        XamlRoot = mainWindow.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
                else
                {
                    // Fallback if DI not available
                    ShowSystemErrorMessage($"Application Error: {ex.Message}");
                }
            }
        }
        catch (Exception dialogEx)
        {
            ExceptionLogger.LogException(dialogEx, "ManualExceptionDialog");
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }
    
    private static Window? GetMainWindow()
    {
        try
        {
            return ((App)Application.Current).MainWindow;
        }
        catch
        {
            return null;
        }
    }
    
    private static Func<Exception, string?, ExceptionDialogViewModel>? GetExceptionDialogFactory()
    {
        try
        {
            var app = (App)Application.Current;
            return app.Services.GetService<Func<Exception, string?, ExceptionDialogViewModel>>();
        }
        catch
        {
            return null;
        }
    }
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    
    private static void ShowSystemErrorMessage(string message)
    {
        try
        {
            MessageBox(IntPtr.Zero, message, "Application Error", 0x10);
        }
        catch
        {
            // Even system message box failed - nothing more we can do
        }
    }
}