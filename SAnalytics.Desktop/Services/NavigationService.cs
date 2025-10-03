using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.Views.Pages;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Implementation of navigation service for WinUI 3 applications.
/// Provides decoupled navigation capabilities for proper MVVM pattern adherence.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly Dictionary<string, Type> _pageRegistry;
    private Frame? _frame;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _pageRegistry = [];
        
        RegisterPages();
    }

    /// <summary>
    /// Sets the frame to use for navigation. Should be called during app initialization.
    /// </summary>
    /// <param name="frame">The frame to use for navigation.</param>
    public void SetFrame(Frame frame)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        Log.Information("Navigation frame set successfully");
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public Type? CurrentPageType => _frame?.CurrentSourcePageType;

    public event EventHandler<NavigationEventArgs>? Navigated;

    public Task<bool> NavigateToAsync<T>(object? parameter = null) where T : Page
    {
        return NavigateToAsync(typeof(T), parameter);
    }

    public async Task<bool> NavigateToAsync(Type pageType, object? parameter = null)
    {
        if (_frame == null)
        {
            Log.Error("Navigation frame not set. Call SetFrame() first.");
            return false;
        }

        try
        {
            var currentPageType = _frame.CurrentSourcePageType;
            
            // Avoid navigating to the same page
            if (currentPageType == pageType)
            {
                Log.Debug("Already on target page {PageType}", pageType.Name);
                return true;
            }

            Log.Information("Navigating from {SourcePage} to {TargetPage}", 
                currentPageType?.Name ?? "None", pageType.Name);

            var result = _frame.Navigate(pageType, parameter);
            
            if (result)
            {
                await Task.CompletedTask; // For potential future async operations
                
                OnNavigated(new NavigationEventArgs(pageType)
                {
                    SourcePageType = currentPageType,
                    Parameter = parameter,
                    IsSuccessful = true
                });
                
                Log.Information("Navigation to {PageType} successful", pageType.Name);
            }
            else
            {
                Log.Warning("Navigation to {PageType} failed", pageType.Name);
                
                OnNavigated(new NavigationEventArgs(pageType)
                {
                    SourcePageType = currentPageType,
                    Parameter = parameter,
                    IsSuccessful = false,
                    ErrorMessage = "Frame.Navigate returned false"
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error navigating to {PageType}", pageType.Name);
            
            OnNavigated(new NavigationEventArgs(pageType)
            {
                SourcePageType = _frame.CurrentSourcePageType,
                Parameter = parameter,
                IsSuccessful = false,
                ErrorMessage = ex.Message
            });
            
            return false;
        }
    }

    public Task<bool> NavigateToAsync(string pageName, object? parameter = null)
    {
        if (!_pageRegistry.TryGetValue(pageName, out var pageType))
        {
            Log.Error("Page with name '{PageName}' not found in registry", pageName);
            return Task.FromResult(false);
        }

        return NavigateToAsync(pageType, parameter);
    }

    public async Task<bool> GoBackAsync()
    {
        if (_frame == null)
        {
            Log.Error("Navigation frame not set. Call SetFrame() first.");
            return false;
        }

        if (!_frame.CanGoBack)
        {
            Log.Debug("Cannot go back - no previous page in navigation stack");
            return false;
        }

        try
        {
            var currentPageType = _frame.CurrentSourcePageType;
            _frame.GoBack();
            
            await Task.CompletedTask; // For potential future async operations
            
            Log.Information("Navigated back from {PageType}", currentPageType?.Name ?? "Unknown");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error navigating back");
            return false;
        }
    }

    public void ClearBackStack()
    {
        if (_frame == null)
        {
            Log.Warning("Navigation frame not set when trying to clear back stack");
            return;
        }

        try
        {
            _frame.BackStack.Clear();
            Log.Information("Navigation back stack cleared");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error clearing navigation back stack");
        }
    }

    /// <summary>
    /// Registers all available pages for navigation by name.
    /// </summary>
    private void RegisterPages()
    {
        // Register pages with their string keys for navigation
        _pageRegistry["Login"] = typeof(LoginPage);
        _pageRegistry["Dashboard"] = typeof(DashboardPage);
        _pageRegistry["Settings"] = typeof(SettingsPage);
        
        Log.Information("Registered {PageCount} pages for navigation", _pageRegistry.Count);
    }

    /// <summary>
    /// Raises the Navigated event.
    /// </summary>
    /// <param name="args">Navigation event arguments.</param>
    protected virtual void OnNavigated(NavigationEventArgs args)
    {
        Navigated?.Invoke(this, args);
    }

    /// <summary>
    /// Cleans up the reference to the navigation frame.
    /// </summary>
    public void Cleanup()
    {
        Log.Information("Cleaning up NavigationService, releasing frame reference.");
        _frame = null;
    }
}

/// <summary>
/// Extension methods for common navigation patterns.
/// </summary>
public static class NavigationServiceExtensions
{
    /// <summary>
    /// Navigates to the Login page.
    /// </summary>
    public static Task<bool> NavigateToLoginAsync(this INavigationService navigationService)
        => navigationService.NavigateToAsync<LoginPage>();
    
    /// <summary>
    /// Navigates to the Dashboard page.
    /// </summary>
    public static Task<bool> NavigateToDashboardAsync(this INavigationService navigationService)
        => navigationService.NavigateToAsync<DashboardPage>();
    
    /// <summary>
    /// Navigates to the Settings page.
    /// </summary>
    public static Task<bool> NavigateToSettingsAsync(this INavigationService navigationService)
        => navigationService.NavigateToAsync<SettingsPage>();
}