using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.Views.Pages;

namespace SAnalytics.Desktop
{
    public sealed partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);
            
            // Get navigation service and configure it
            var app = (App)Application.Current;
            _navigationService = app.Services.GetRequiredService<INavigationService>();
            _navigationService.SetFrame(ContentFrame);
            
            // Navigate to login page
            _ = _navigationService.NavigateToAsync<LoginPage>();
        }

        public async void NavigateToDashboard()
        {
            await _navigationService.NavigateToAsync<DashboardPage>();
        }

        public async void NavigateToLogin()
        {
            await _navigationService.NavigateToAsync<LoginPage>();
        }
    }
}
