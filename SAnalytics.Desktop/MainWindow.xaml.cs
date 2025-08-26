using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Views.Pages;

namespace SAnalytics.Desktop
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);
            ContentFrame.Navigate(typeof(LoginPage));
        }

        public void NavigateToDashboard()
        {
            ContentFrame.Navigate(typeof(DashboardPage));
        }

        public void NavigateToLogin()
        {
            ContentFrame.Navigate(typeof(LoginPage));
        }
    }
}
