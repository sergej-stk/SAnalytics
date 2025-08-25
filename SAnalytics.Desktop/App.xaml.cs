using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Core;
using SAnalytics.Desktop.Views.Pages;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SAnalytics.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private static IHost? _host;

        public App()
        {
            InitializeComponent();
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddViewModels();
            services.AddServices();
        }

        public static T GetService<T>() where T : class
        {
            return _host?.Services.GetRequiredService<T>() 
                ?? throw new InvalidOperationException("Host not initialized");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new LoginWindow();
            _window.Activate();
        }
    }
}
