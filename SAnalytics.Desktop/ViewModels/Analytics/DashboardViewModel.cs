using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Core.ViewModels;
using SAnalytics.Desktop.Services;
using SAnalytics.Desktop.Views.Pages;
using System;
using System.Globalization;
using System.Threading.Tasks;

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

    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    public DashboardViewModel(
        ILocalizationService localizationService,
        ILogger<DashboardViewModel> logger,
        IAuthenticationService authenticationService,
        INavigationService navigationService)
        : base(localizationService, logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
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
        await ExecuteWithBusyStateAsync(async (cancellationToken) =>
        {
            // Simulate data loading
            await Task.Delay(2000, cancellationToken);
            DatasetName = "Beispiel Dataset";
            RecordCount = 1000;
            LastUpdated = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            
            Logger.LogInformation("Dashboard data loaded: {DatasetName} with {RecordCount} records", DatasetName, RecordCount);
        }, GetLocalizedString("LoadingData"));
    }
    
    [RelayCommand]
    private void ExportData()
    { 
        throw new NotImplementedException("ExportData feature is not yet implemented. This will be available in a future version.");
    }
    
    [RelayCommand]
    private async Task LogoutAsync()
    {
        await ExecuteWithBusyStateAsync(async (cancellationToken) =>
        {
            await _authenticationService.SignOutAsync(cancellationToken);
            Logger.LogInformation("User logged out from dashboard");
            
            // Navigate to login page
            await _navigationService.NavigateToAsync<LoginPage>();
        }, GetLocalizedString("LoggingOut"));
    }
}