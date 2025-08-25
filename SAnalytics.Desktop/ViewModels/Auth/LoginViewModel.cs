using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAnalytics.Desktop.Core.ViewModels;
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

    public LoginViewModel()
    {
        UpdateLocalizedStrings();
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        Title = GetLocalizedString("LoginTitle");
        UsernameLabel = GetLocalizedString("Username");
        PasswordLabel = GetLocalizedString("Password");
        RememberMeLabel = GetLocalizedString("RememberMe");
        LoginButtonText = GetLocalizedString("Login");
        ForgotPasswordText = GetLocalizedString("ForgotPassword");
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
            await Task.Delay(2000);
            
            if (Username == "admin" && Password == "admin")
            {
                // Navigation zur Hauptapp würde hier passieren
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
}