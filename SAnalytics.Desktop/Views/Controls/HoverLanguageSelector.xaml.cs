using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SAnalytics.Desktop.ViewModels.Settings;
using System;

namespace SAnalytics.Desktop.Views.Controls
{
    public sealed partial class HoverLanguageSelector : UserControl
    {
        public SettingsViewModel ViewModel { get; }

        public HoverLanguageSelector()
        {
            var app = (App)Application.Current;
            ViewModel = app.Services.GetRequiredService<SettingsViewModel>();
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }
    }
}