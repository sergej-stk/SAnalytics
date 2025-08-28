using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SAnalytics.Desktop.Core.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace SAnalytics.Desktop.ViewModels.Analytics;

public partial class DashboardViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _datasetName = string.Empty;
    
    [ObservableProperty]
    private int _recordCount;
    
    [ObservableProperty]
    private string _lastUpdated = string.Empty;

    [ObservableProperty]
    private string _dashboardText = string.Empty;

    [ObservableProperty]
    private string _logoutText = string.Empty;

    [ObservableProperty]
    private string _datasetInformationText = string.Empty;

    [ObservableProperty]
    private string _datasetNameLabel = string.Empty;

    [ObservableProperty]
    private string _recordCountLabel = string.Empty;

    [ObservableProperty]
    private string _lastUpdatedLabel = string.Empty;

    [ObservableProperty]
    private string _loadDataText = string.Empty;

    [ObservableProperty]
    private string _exportDataText = string.Empty;

    public DashboardViewModel()
    {
        UpdateLocalizedStrings();
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        Title = GetLocalizedString("Dashboard");
        DashboardText = GetLocalizedString("Dashboard");
        LogoutText = GetLocalizedString("Logout");
        DatasetInformationText = GetLocalizedString("DatasetInformation");
        DatasetNameLabel = GetLocalizedString("DatasetName");
        RecordCountLabel = GetLocalizedString("RecordCount");
        LastUpdatedLabel = GetLocalizedString("LastUpdated");
        LoadDataText = GetLocalizedString("LoadData");
        ExportDataText = GetLocalizedString("ExportData");

        // Initialize default values if empty
        if (string.IsNullOrEmpty(DatasetName))
        {
            DatasetName = GetLocalizedString("ExceptionDialog_Unknown");
        }
        if (string.IsNullOrEmpty(LastUpdated))
        {
            LastUpdated = GetLocalizedString("ExceptionDialog_Unknown");
        }
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