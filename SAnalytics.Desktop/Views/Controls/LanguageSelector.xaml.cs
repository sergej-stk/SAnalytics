using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Settings;

namespace SAnalytics.Desktop.Views.Controls;

public sealed partial class LanguageSelector : UserControl
{
    public SettingsViewModel ViewModel { get; }

    public LanguageSelector()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}