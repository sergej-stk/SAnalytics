using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Analytics;

namespace SAnalytics.Desktop.Views.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        ViewModel = App.GetService<DashboardViewModel>();
        InitializeComponent();
    }
}