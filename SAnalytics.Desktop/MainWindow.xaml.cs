using Microsoft.UI.Xaml;
using SAnalytics.Desktop.ViewModels.Analytics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SAnalytics.Desktop
{
    public sealed partial class MainWindow : Window
    {
        public DashboardViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = App.GetService<DashboardViewModel>();
            InitializeComponent();
        }
    }
}
