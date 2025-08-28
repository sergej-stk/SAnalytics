using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Controls;
using System;

namespace SAnalytics.Desktop.Views.Controls
{
    public sealed partial class ThemeSelector : UserControl
    {
        public ThemeSelectorViewModel ViewModel { get; }

        public ThemeSelector()
        {
            var app = (App)Application.Current;
            ViewModel = app.Services.GetRequiredService<ThemeSelectorViewModel>();
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}