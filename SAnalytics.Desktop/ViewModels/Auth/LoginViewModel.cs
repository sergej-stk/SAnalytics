using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAnalytics.Desktop.Core.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
    private string _errorMessage = string.Empty;
    
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

    public LoginViewModel()
    {
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
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = GetLocalizedString("LoginError_FieldsRequired");
            return;
        }

        IsBusy = true;
        IsLoginAnimating = true;
        ErrorMessage = string.Empty;
        
        try
        {
            await Task.CompletedTask;

            if (Username == "admin" && Password == "admin")
            {
                var mainWindow = ((App)Application.Current).MainWindow;
                mainWindow?.NavigateToDashboard();
            }
            else
            {
                ErrorMessage = GetLocalizedString("LoginError_InvalidCredentials");
            }
        }
        finally
        {
            IsBusy = false;
            IsLoginAnimating = false;
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
        ErrorMessage = string.Empty;
        IsBusy = false;
    }
}