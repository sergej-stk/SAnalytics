using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Controls;

namespace SAnalytics.Desktop.Views.Controls
{
    public sealed partial class ThemeSelector : UserControl
    {
        public ThemeSelectorViewModel ViewModel { get; }

        public ThemeSelector()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<ThemeSelectorViewModel>();
            this.DataContext = ViewModel;
        }
    }
}