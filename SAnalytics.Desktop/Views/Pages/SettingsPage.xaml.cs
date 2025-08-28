using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Settings;
using SAnalytics.Desktop.Services;
using System;
using System.Linq;

namespace SAnalytics.Desktop.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;

        public SettingsPage()
        {
            var app = (App)Application.Current;
            ViewModel = app.Services.GetRequiredService<SettingsViewModel>();
            _navigationService = app.Services.GetRequiredService<INavigationService>();
            _authenticationService = app.Services.GetRequiredService<IAuthenticationService>();
            
            this.InitializeComponent();
            this.DataContext = ViewModel;
            MainNavigationView.ItemInvoked += OnNavigationViewItemInvoked;
        }

        private async void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // Already on settings, do nothing
                return;
            }
            
            if (args.InvokedItemContainer?.Tag is string tag)
            {
                switch (tag)
                {
                    case "Dashboard":
                        await _navigationService.NavigateToAsync<DashboardPage>();
                        break;
                    case "Logout":
                        await _authenticationService.SignOutAsync();
                        await _navigationService.NavigateToAsync<LoginPage>();
                        break;
                }
            }
        }
    }
}