using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Core;
using SAnalytics.Desktop.Core.Exceptions;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.Views.Pages;
using System;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SAnalytics.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow? _window;
        private static IHost? _host;

        public MainWindow? MainWindow => _window;

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

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Initialize global exception handling first
            WinUI3ExceptionHandler.Initialize(this);
            
            // Start periodic GC to catch unobserved task exceptions
            StartPeriodicGarbageCollection();
            
            // Load app settings (commented out for now)
            // var appSettingsService = GetService<IAppSettingsService>();
            // await appSettingsService.LoadSettingsAsync();
            
            // Initialize theme service
            var themeService = GetService<IThemeService>();
            themeService.Initialize();
            
            _window = new MainWindow();
            _window.Activate();
        }

        private void StartPeriodicGarbageCollection()
        {
            // Force periodic GC to catch unobserved task exceptions
            var timer = new Timer(
                callback: _ => GC.Collect(0, GCCollectionMode.Optimized),
                state: null,
                dueTime: TimeSpan.FromMinutes(5),
                period: TimeSpan.FromMinutes(5));
        }
    }
}
