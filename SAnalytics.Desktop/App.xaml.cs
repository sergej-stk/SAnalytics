using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Core;
using SAnalytics.Desktop.Services;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SAnalytics.Desktop;

public partial class App : Application
{
    private MainWindow? _window;
    private IHost? _host;

    public MainWindow? MainWindow => 
        _window;

    public IServiceProvider Services => _host?.Services ?? 
        throw new InvalidOperationException("Application not initialized");

    public App()
    {
        InitializeLogging();
        InitializeComponent();

        UnhandledException += (_, e) =>
        {
            Log.Fatal(e.Exception, "An unhandled exception occured.");
            Log.CloseAndFlush();
        };
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            await InitializeHostAsync();
            await InitializeCoreServicesAsync();

            _window = new MainWindow();
            _window.Activate();
            _window.Closed += async (_, _) => await ShutdownAsync();

            Log.Debug("Application launched successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Critical application launch failure.");
            await ShutdownAsync();
            throw;
        }
    }

    private Task InitializeHostAsync()
    {
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) => services.AddApplicationServices())
            .Build();

        return _host.StartAsync();
    }

    private async Task InitializeCoreServicesAsync()
    {
        try
        {
            if (_host?.Services == null)
                throw new InvalidOperationException("Host services not initialized");

            var themeService = _host.Services.GetRequiredService<IThemeService>();
            themeService.Initialize();
            Log.Debug("Theme service initialized");
            
            var configService = _host.Services.GetRequiredService<IAppConfigurationService>();
            await configService.ReloadAsync();
            Log.Debug("Configuration service initialized");
            
            var authService = _host.Services.GetRequiredService<IAuthenticationService>();
            var autoLoginResult = await authService.TryAutoAuthenticateAsync();
            if (autoLoginResult.IsSuccess)
            {
                Log.Information("User auto-authenticated successfully");
            }
            else
            {
                Log.Debug("Auto-authentication not available or failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing core services");
            throw;
        }
    }

    public async Task ShutdownAsync()
    {
        try
        {
            if (_host is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                _host?.Dispose();

            Log.Information("Application shut down successfully");

        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during application shutdown");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private void InitializeLogging() =>
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.Console()
            .CreateLogger();
}
