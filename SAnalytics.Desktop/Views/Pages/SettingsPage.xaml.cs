using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Analytics;
using SAnalytics.Desktop.ViewModels.Settings;
using System.Linq;

namespace SAnalytics.Desktop.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<SettingsViewModel>();
            this.DataContext = ViewModel;
            MainNavigationView.ItemInvoked += OnNavigationViewItemInvoked;
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var mainWindow = ((App)Application.Current).MainWindow;
            
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
                        mainWindow?.NavigateToDashboard();
                        break;
                    case "Logout":
                        // Use DashboardViewModel's logout functionality
                        var dashboardViewModel = App.GetService<DashboardViewModel>();
                        dashboardViewModel.LogoutCommand.Execute(null);
                        break;
                }
            }
        }
    }
}