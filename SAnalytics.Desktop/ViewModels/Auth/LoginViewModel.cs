using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.Core.ViewModels;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.Views.Pages;
using Serilog;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SAnalytics.Desktop.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private string _password = string.Empty;
    
    [ObservableProperty]
    private bool _rememberMe;
    
    [ObservableProperty]
    private bool _isLoginAnimating;

    [ObservableProperty]
    private string _usernameLabel = string.Empty;

    [ObservableProperty]
    private string _passwordLabel = string.Empty;

    [ObservableProperty]
    private string _rememberMeLabel = string.Empty;

    [ObservableProperty]
    private string _loginButtonText = string.Empty;

    [ObservableProperty]
    private string _forgotPasswordText = string.Empty;

    [ObservableProperty]
    private bool _showDebugButton;

    [ObservableProperty]
    private string _debugButtonText = string.Empty;

    [ObservableProperty]
    private string _debugDialogTitle = string.Empty;

    [ObservableProperty]
    private string _debugDialogContent = string.Empty;

    [ObservableProperty]
    private string _debugDialogYes = string.Empty;

    [ObservableProperty]
    private string _debugDialogCancel = string.Empty;

    [ObservableProperty]
    private string _debugModeInfoText = string.Empty;

    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    public LoginViewModel(
        ILocalizationService localizationService,
        IAuthenticationService authenticationService,
        INavigationService navigationService)
        : base(localizationService)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        UpdateLocalizedStrings();
        InitializeDebugMode();
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture) =>
        UpdateLocalizedStrings();

    private void UpdateLocalizedStrings()
    {
        Title = GetLocalizedString("LoginTitle");
        UsernameLabel = GetLocalizedString("Username");
        PasswordLabel = GetLocalizedString("Password");
        RememberMeLabel = GetLocalizedString("RememberMe");
        LoginButtonText = GetLocalizedString("Login");
        ForgotPasswordText = GetLocalizedString("ForgotPassword");
        DebugButtonText = GetLocalizedString("DebugButtonText");
        DebugDialogTitle = GetLocalizedString("DebugDialogTitle");
        DebugDialogContent = GetLocalizedString("DebugDialogContent");
        DebugDialogYes = GetLocalizedString("DebugDialogYes");
        DebugDialogCancel = GetLocalizedString("DebugDialogCancel");
        DebugModeInfoText = GetLocalizedString("DebugModeInfoText");
    }

    private void InitializeDebugMode()
    {
#if DEBUG
        ShowDebugButton = true;
#else
        ShowDebugButton = false;
#endif
    }

    [RelayCommand]
    private async Task ShowDebugDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = DebugDialogTitle,
            Content = DebugDialogContent,
            PrimaryButtonText = DebugDialogYes,
            SecondaryButtonText = DebugDialogCancel,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = ((App)Application.Current).MainWindow?.Content?.XamlRoot
        };

        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            Username = "admin";
            Password = "admin";
            ErrorMessage = string.Empty;
            
            // Auto-Login ausführen
            await LoginAsync();
        }
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        var success = await ExecuteWithBusyStateAsync(async (cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                SetError(GetLocalizedString("LoginError_FieldsRequired"));
                return;
            }

            IsLoginAnimating = true;
            ClearError();

            try
            {
                var result = await _authenticationService.AuthenticateAsync(Username, Password, RememberMe, cancellationToken);

                if (result.IsSuccess && result.User != null)
                {
                    Log.Information("User {Username} logged in successfully", Username);
                    
                    // Navigate to dashboard using navigation service
                    await _navigationService.NavigateToAsync<DashboardPage>();
                }
                else
                {
                    var errorMessage = result.ErrorMessage ?? GetLocalizedString("LoginError_InvalidCredentials");
                    SetError(errorMessage);
                    Log.Warning("Login failed for user {Username}: {Error}", Username, errorMessage);
                }
            }
            finally
            {
                IsLoginAnimating = false;
            }
        }, GetLocalizedString("LoggingIn"));

        // Reset password on failure for security
        if (!success)
        {
            Password = string.Empty;
        }
    }
    
    [RelayCommand]
    private void ForgotPassword()
    {
        // Passwort vergessen Logic
    }
    
    public void ResetForm()
    {
        Password = string.Empty;
        ClearError();
        SetBusyState(false);
    }
}