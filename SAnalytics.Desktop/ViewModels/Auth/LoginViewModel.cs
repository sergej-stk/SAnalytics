using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAnalytics.Desktop.Core.ViewModels;
using System;
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

    public LoginViewModel()
    {
        Title = "SAnalytics Login";
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Benutzername und Passwort sind erforderlich";
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
                ErrorMessage = "Ungültige Anmeldedaten";
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