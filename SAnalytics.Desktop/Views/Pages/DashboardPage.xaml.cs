using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Settings;
using System.Linq;

namespace SAnalytics.Desktop.Views.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public DashboardPage()
    {
        ViewModel = App.GetService<DashboardViewModel>();
        SettingsViewModel = App.GetService<SettingsViewModel>();
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