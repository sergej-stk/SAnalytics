using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SAnalytics.Desktop.ViewModels.Auth;

namespace SAnalytics.Desktop.Views.Pages;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        
        // Start particle animations when page loads
        this.Loaded += (sender, e) => ParticleAnimation.Begin();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.ResetForm();
        
        // Restart animations when navigating to this page
        ParticleAnimation.Begin();
    }
}