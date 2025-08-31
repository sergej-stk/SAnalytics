using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Core;

using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.Views.Pages;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SAnalytics.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// Uses proper dependency injection and async initialization patterns.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow? _window;
        private IHost? _host;
        private ILogger<App>? _logger;

        public MainWindow? MainWindow => _window;
        public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Application not initialized");
        public static bool IsShuttingDown { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        private async Task InitializeApplicationAsync()
        {
            try
            {
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices(ConfigureServices)
                    .Build();

                await _host.StartAsync();
                
                _logger = _host.Services.GetRequiredService<ILogger<App>>();
                _logger.LogInformation("Application host initialized successfully");
            }
            catch (Exception ex)
            {
                // Fall back to console logging if DI logger isn't available
                System.Diagnostics.Debug.WriteLine($"Failed to initialize application: {ex}");
                throw;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Use the comprehensive service registration from ServiceExtensions
            services.AddApplicationServices();
        }


        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                // Initialize the application host and DI container
                await InitializeApplicationAsync();

                
                
                // Initialize core services
                await InitializeCoreServicesAsync();
                
                // Create and activate the main window
                _window = new MainWindow();
                _window.Activate();
                _window.Closed += Window_Closed;
                
                _logger?.LogInformation("Application launched successfully");
            }
            catch (Exception ex)
            {
                // Critical failure - log and show error dialog if possible
                System.Diagnostics.Debug.WriteLine($"Critical application launch failure: {ex}");
                
                // Try to show a basic error message to the user
                try
                {
                    var errorWindow = new MainWindow();
                    // Could show a critical error dialog here
                    errorWindow.Activate();
                }
                catch
                {
                    // Ultimate fallback - just crash
                    throw;
                }
            }
        }

        private async Task InitializeCoreServicesAsync()
        {
            try
            {
                if (_host?.Services == null)
                    throw new InvalidOperationException("Host services not initialized");

                // Initialize theme service
                var themeService = _host.Services.GetRequiredService<IThemeService>();
                themeService.Initialize();
                _logger?.LogDebug("Theme service initialized");
                
                // Initialize configuration service and load saved settings
                var configService = _host.Services.GetRequiredService<IAppConfigurationService>();
                await configService.ReloadAsync();
                _logger?.LogDebug("Configuration service initialized");
                
                // Initialize authentication service for auto-login
                var authService = _host.Services.GetRequiredService<IAuthenticationService>();
                var autoLoginResult = await authService.TryAutoAuthenticateAsync();
                if (autoLoginResult.IsSuccess)
                {
                    _logger?.LogInformation("User auto-authenticated successfully");
                }
                else
                {
                    _logger?.LogDebug("Auto-authentication not available or failed");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing core services");
                throw;
            }
        }

        private async void Window_Closed(object sender, WindowEventArgs args)
        {
            // Clean up services that hold UI references BEFORE shutting down the host
            var navigationService = Services.GetService<INavigationService>();
            navigationService?.Cleanup();

            await ShutdownAsync();
        }

        /// <summary>
        /// Cleans up resources when the application is shutting down.
        /// </summary>
        public async Task ShutdownAsync()
        {
            IsShuttingDown = true;
            try
            {
                if (_host != null)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                    _host.Dispose();
                    _logger?.LogInformation("Application shut down successfully");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application shutdown");
            }
        }
    }
}
