using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Controls;

namespace SAnalytics.Desktop.Views.Controls
{
    public sealed partial class HoverThemeSelector : UserControl
    {
        public ThemeSelectorViewModel ViewModel { get; }

        public HoverThemeSelector()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<ThemeSelectorViewModel>();
            this.DataContext = ViewModel;
        }
    }
}