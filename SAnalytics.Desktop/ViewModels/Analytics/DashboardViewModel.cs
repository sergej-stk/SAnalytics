using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAnalytics.Desktop.Core.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace SAnalytics.Desktop.ViewModels.Analytics;

public partial class DashboardViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _datasetName = "Kein Dataset geladen";
    
    [ObservableProperty]
    private int _recordCount;
    
    [ObservableProperty]
    private string _lastUpdated = "Nie";

    public DashboardViewModel()
    {
        Title = "Dashboard";
    }
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(2000);
            DatasetName = "Beispiel Dataset";
            RecordCount = 1000;
            LastUpdated = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void ExportData()
    { 
        throw new NotImplementedException("ExportData is not implemented yet.");
    }
    
    [RelayCommand]
    private void Logout()
    {
        var mainWindow = ((App)Application.Current).MainWindow;
        mainWindow?.NavigateToLogin();
    }
}