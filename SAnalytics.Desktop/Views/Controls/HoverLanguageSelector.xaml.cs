using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Settings;

namespace SAnalytics.Desktop.Views.Controls
{
    public sealed partial class HoverLanguageSelector : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public HoverLanguageSelector()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<SettingsViewModel>();
            this.DataContext = ViewModel;
        }
    }
}