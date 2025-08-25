using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Auth;

namespace SAnalytics.Desktop.Views.Pages;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        
        // Start login animation when page loads
        Loaded += (s, e) => LoginAnimation.Begin();
        
        // Subscribe to ViewModel events for animations
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsLoginAnimating) && ViewModel.IsLoginAnimating)
            {
                LoginAnimation.Begin();
            }
            else if (e.PropertyName == nameof(ViewModel.ErrorMessage) && !string.IsNullOrEmpty(ViewModel.ErrorMessage))
            {
                ErrorShakeAnimation.Begin();
            }
        };
    }
}