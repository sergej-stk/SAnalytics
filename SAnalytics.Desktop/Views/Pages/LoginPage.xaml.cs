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
    }
}