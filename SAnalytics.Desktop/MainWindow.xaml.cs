using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.Views.Pages;

namespace SAnalytics.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        
        var app = (App)Application.Current;
        _navigationService = app.Services.GetRequiredService<INavigationService>();
        _navigationService.SetFrame(ContentFrame);
        
        _ = _navigationService.NavigateToAsync<LoginPage>();
    }
}
