using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Settings;
using System;
using System.Linq;

namespace SAnalytics.Desktop.Views.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public DashboardPage()
    {
        var app = (App)Application.Current;
        ViewModel = app.Services.GetRequiredService<DashboardViewModel>();
        SettingsViewModel = app.Services.GetRequiredService<SettingsViewModel>();
        InitializeComponent();
        MainNavigationView.ItemInvoked += OnNavigationViewItemInvoked;
        MainNavigationView.SelectedItem = MainNavigationView.MenuItems.FirstOrDefault();
    }

    private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            ShowSettings();
            return;
        }
        
        if (args.InvokedItemContainer?.Tag is string tag)
        {
            switch (tag)
            {
                case "Logout":
                    ViewModel.LogoutCommand.Execute(null);
                    break;
                case "Dashboard":
                    ShowDashboard();
                    break;
            }
        }
    }

    private void ShowDashboard()
    {
        DashboardContent.Visibility = Visibility.Visible;
        SettingsContent.Visibility = Visibility.Collapsed;
        MainNavigationView.SelectedItem = MainNavigationView.MenuItems.FirstOrDefault();
    }

    private void ShowSettings()
    {
        DashboardContent.Visibility = Visibility.Collapsed;
        SettingsContent.Visibility = Visibility.Visible;
    }
}