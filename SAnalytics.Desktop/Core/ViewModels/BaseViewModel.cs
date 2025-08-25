using CommunityToolkit.Mvvm.ComponentModel;

namespace SAnalytics.Desktop.Core.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;
    
    [ObservableProperty]
    private string _title = string.Empty;
}