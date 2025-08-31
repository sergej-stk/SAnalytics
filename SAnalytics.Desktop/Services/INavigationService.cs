using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.Services;

/// <summary>
/// Service for handling navigation between pages in a decoupled manner.
/// Enables proper MVVM pattern by removing direct UI dependencies from ViewModels.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Sets the frame to use for navigation. Should be called during app initialization.
    /// </summary>
    /// <param name="frame">The frame to use for navigation.</param>
    void SetFrame(Frame frame);
    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <typeparam name="T">The page type to navigate to.</typeparam>
    /// <param name="parameter">Optional parameter to pass to the destination page.</param>
    /// <returns>A task representing the navigation operation.</returns>
    Task<bool> NavigateToAsync<T>(object? parameter = null) where T : Page;
    
    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <param name="pageType">The page type to navigate to.</param>
    /// <param name="parameter">Optional parameter to pass to the destination page.</param>
    /// <returns>A task representing the navigation operation.</returns>
    Task<bool> NavigateToAsync(Type pageType, object? parameter = null);
    
    /// <summary>
    /// Navigates to the specified page by name.
    /// </summary>
    /// <param name="pageName">The name/key of the page to navigate to.</param>
    /// <param name="parameter">Optional parameter to pass to the destination page.</param>
    /// <returns>A task representing the navigation operation.</returns>
    Task<bool> NavigateToAsync(string pageName, object? parameter = null);
    
    /// <summary>
    /// Gets whether the navigation service can go back.
    /// </summary>
    bool CanGoBack { get; }
    
    /// <summary>
    /// Navigates back to the previous page if possible.
    /// </summary>
    /// <returns>A task representing the navigation operation.</returns>
    Task<bool> GoBackAsync();
    
    /// <summary>
    /// Gets the current page type.
    /// </summary>
    Type? CurrentPageType { get; }
    
    /// <summary>
    /// Clears the navigation back stack.
    /// </summary>
    void ClearBackStack();
    
    /// <summary>
    /// Event raised when navigation occurs.
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;

    /// <summary>
    /// Cleans up references to UI elements to prevent memory leaks and crashes on shutdown.
    /// </summary>
    void Cleanup();
}

/// <summary>
/// Event arguments for navigation events.
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public Type? SourcePageType { get; init; }
    public Type TargetPageType { get; init; } = default!;
    public object? Parameter { get; init; }
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    
    public NavigationEventArgs(Type targetPageType)
    {
        TargetPageType = targetPageType;
    }
}