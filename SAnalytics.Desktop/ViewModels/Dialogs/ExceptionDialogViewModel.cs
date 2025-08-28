using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using SAnalytics.Desktop.Core.ViewModels;
using SAnalytics.Desktop.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SAnalytics.Desktop.ViewModels.Dialogs;

public partial class ExceptionDialogViewModel : BaseViewModel
{
    private readonly Exception _exception;
    private readonly string? _userMessage;

    [ObservableProperty]
    private string _dialogTitle = string.Empty;

    [ObservableProperty]
    private string _primaryButtonText = string.Empty;

    [ObservableProperty]
    private string _secondaryButtonText = string.Empty;

    [ObservableProperty]
    private string _closeButtonText = string.Empty;

    [ObservableProperty]
    private string _errorHeaderText = string.Empty;

    [ObservableProperty]
    private string _errorText = string.Empty;

    [ObservableProperty]
    private string _userActionsTitle = string.Empty;

    [ObservableProperty]
    private string _userActionsMessage = string.Empty;

    [ObservableProperty]
    private string _feedbackMessage = string.Empty;

    [ObservableProperty]
    private string _exceptionDetailsHeader = string.Empty;

    [ObservableProperty]
    private string _exceptionDetailsText = string.Empty;

    [ObservableProperty]
    private string _systemInfoHeader = string.Empty;

    [ObservableProperty]
    private string _systemInfoText = string.Empty;

    [ObservableProperty]
    private string _debugInfoHeader = string.Empty;

    [ObservableProperty]
    private string _memoryInfoLabel = string.Empty;

    [ObservableProperty]
    private string _memoryInfoText = string.Empty;

    [ObservableProperty]
    private string _threadInfoLabel = string.Empty;

    [ObservableProperty]
    private string _threadInfoText = string.Empty;

    [ObservableProperty]
    private Visibility _showDebugInfo;

    private string _fullReport = string.Empty;

    public ExceptionDialogViewModel(
        Exception exception, 
        string? userMessage,
        ILocalizationService localizationService,
        ILogger<ExceptionDialogViewModel> logger)
        : base(localizationService, logger)
    {
        _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        _userMessage = userMessage;

        // Set debug mode visibility
#if DEBUG
        ShowDebugInfo = Visibility.Visible;
#else
        ShowDebugInfo = Visibility.Collapsed;
#endif

        InitializeData();
        UpdateLocalizedStrings();
        
        Logger.LogWarning("ExceptionDialogViewModel created for exception: {ExceptionType}", exception.GetType().Name);
    }

    private void InitializeData()
    {
        ErrorText = _userMessage ?? GetUserFriendlyMessage(_exception);
        ExceptionDetailsText = BuildExceptionDetails();
        SystemInfoText = BuildSystemInfo();
        MemoryInfoText = BuildMemoryInfo();
        ThreadInfoText = BuildThreadInfo();
        _fullReport = BuildFullReport();
    }

    protected override void OnLanguageChanged(object? sender, CultureInfo culture)
    {
        UpdateLocalizedStrings();
    }

    private void UpdateLocalizedStrings()
    {
        DialogTitle = GetLocalizedString("ExceptionDialog_Title");
        PrimaryButtonText = GetLocalizedString("ExceptionDialog_CopyDetails");
        SecondaryButtonText = GetLocalizedString("ExceptionDialog_SaveReport");
        CloseButtonText = GetLocalizedString("Close");
        ErrorHeaderText = GetLocalizedString("ExceptionDialog_ErrorHeader");
        UserActionsTitle = GetLocalizedString("ExceptionDialog_UserActionsTitle");
        UserActionsMessage = GetLocalizedString("ExceptionDialog_UserActionsMessage");
        ExceptionDetailsHeader = GetLocalizedString("ExceptionDialog_DetailsHeader");
        SystemInfoHeader = GetLocalizedString("ExceptionDialog_SystemInfoHeader");
        DebugInfoHeader = GetLocalizedString("ExceptionDialog_DebugInfoHeader");
        MemoryInfoLabel = GetLocalizedString("ExceptionDialog_MemoryInfoLabel");
        ThreadInfoLabel = GetLocalizedString("ExceptionDialog_ThreadInfoLabel");

        // Rebuild full report with new localized strings
        _fullReport = BuildFullReport();
    }

    private string GetUserFriendlyMessage(Exception ex)
    {
#if DEBUG
        return ex.Message; // Show actual message in debug
#else
        // Generic messages for release mode for security
        return ex switch
        {
            FileNotFoundException => GetLocalizedString("ExceptionDialog_FileNotFound"),
            UnauthorizedAccessException => GetLocalizedString("ExceptionDialog_AccessDenied"),
            TimeoutException => GetLocalizedString("ExceptionDialog_Timeout"),
            ArgumentException => GetLocalizedString("ExceptionDialog_InvalidInput"),
            InvalidOperationException => GetLocalizedString("ExceptionDialog_InvalidOperation"),
            NotSupportedException => GetLocalizedString("ExceptionDialog_NotSupported"),
            _ => GetLocalizedString("ExceptionDialog_UnexpectedError")
        };
#endif
    }

    private string BuildExceptionDetails()
    {
        var details = new StringBuilder();
        
        var ex = _exception;
        int level = 0;
        
        while (ex != null)
        {
            if (level > 0)
                details.AppendLine($"\n--- {GetLocalizedString("ExceptionDialog_InnerException")} {level} ---");
                
            details.AppendLine($"{GetLocalizedString("ExceptionDialog_Type")}: {ex.GetType().FullName}");
            details.AppendLine($"{GetLocalizedString("ExceptionDialog_Message")}: {ex.Message}");
            
            if (!string.IsNullOrEmpty(ex.Source))
                details.AppendLine($"{GetLocalizedString("ExceptionDialog_Source")}: {ex.Source}");
            
            if (ex.TargetSite != null)
                details.AppendLine($"{GetLocalizedString("ExceptionDialog_TargetMethod")}: {ex.TargetSite.Name}");
            
            // Include full stack trace in debug mode
#if DEBUG
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                details.AppendLine($"{GetLocalizedString("ExceptionDialog_StackTrace")}:");
                details.AppendLine(ex.StackTrace);
            }
            
            // Additional exception data
            if (ex.Data.Count > 0)
            {
                details.AppendLine($"{GetLocalizedString("ExceptionDialog_AdditionalData")}:");
                foreach (System.Collections.DictionaryEntry item in ex.Data)
                {
                    details.AppendLine($"  {item.Key}: {item.Value}");
                }
            }
#else
            // Limited stack trace in release mode
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                var lines = ex.StackTrace.Split('\n');
                var limitedTrace = string.Join('\n', lines.Take(3));
                details.AppendLine($"{GetLocalizedString("ExceptionDialog_StackTraceLimited")}:");
                details.AppendLine(limitedTrace);
                if (lines.Length > 3)
                    details.AppendLine($"... ({GetLocalizedString("ExceptionDialog_MoreFramesHidden")})");
            }
#endif
            
            ex = ex.InnerException;
            level++;
        }
        
        return details.ToString();
    }

    private string BuildSystemInfo()
    {
        var info = new StringBuilder();
        var entryAssembly = Assembly.GetEntryAssembly();
        
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_Application")}: {entryAssembly?.GetName().Name ?? GetLocalizedString("ExceptionDialog_Unknown")}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_Version")}: {entryAssembly?.GetName().Version?.ToString() ?? GetLocalizedString("ExceptionDialog_Unknown")}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_Timestamp")}: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
#if DEBUG
        // Detailed system info in debug mode
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_BuildConfiguration")}: Debug");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_AssemblyLocation")}: {entryAssembly?.Location ?? GetLocalizedString("ExceptionDialog_Unknown")}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_OSVersion")}: {Environment.OSVersion}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_DotNetVersion")}: {Environment.Version}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_64BitOS")}: {Environment.Is64BitOperatingSystem}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_64BitProcess")}: {Environment.Is64BitProcess}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_ProcessorCount")}: {Environment.ProcessorCount}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_MachineName")}: {Environment.MachineName}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_UserName")}: {Environment.UserName}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_WorkingDirectory")}: {Environment.CurrentDirectory}");
#else
        // Limited system info in release mode
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_BuildConfiguration")}: Release");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_OS")}: {Environment.OSVersion.Platform}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_DotNet")}: {Environment.Version.Major}.{Environment.Version.Minor}");
        info.AppendLine($"{GetLocalizedString("ExceptionDialog_Processors")}: {Environment.ProcessorCount}");
#endif
        
        return info.ToString();
    }

    private string BuildMemoryInfo()
    {
        var info = new StringBuilder();
        
        try
        {
            var process = Process.GetCurrentProcess();
            
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_WorkingMemory")}: {FormatBytes(process.WorkingSet64)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_PrivateMemory")}: {FormatBytes(process.PrivateMemorySize64)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_GCTotalMemory")}: {FormatBytes(GC.GetTotalMemory(false))}");
            
#if DEBUG
            // Detailed memory info in debug mode
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_PeakWorkingMemory")}: {FormatBytes(process.PeakWorkingSet64)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_VirtualMemory")}: {FormatBytes(process.VirtualMemorySize64)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_PeakVirtualMemory")}: {FormatBytes(process.PeakVirtualMemorySize64)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_GCGen0Collections")}: {GC.CollectionCount(0)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_GCGen1Collections")}: {GC.CollectionCount(1)}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_GCGen2Collections")}: {GC.CollectionCount(2)}");
#endif
        }
        catch (Exception ex)
        {
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_MemoryInfoUnavailable")}: {ex.Message}");
        }
        
        return info.ToString();
    }

    private string BuildThreadInfo()
    {
        var info = new StringBuilder();
        
        try
        {
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_CurrentThreadId")}: {Thread.CurrentThread.ManagedThreadId}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_BackgroundThread")}: {Thread.CurrentThread.IsBackground}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_ThreadPoolThread")}: {Thread.CurrentThread.IsThreadPoolThread}");
            
#if DEBUG
            // Detailed thread info in debug mode
            var process = Process.GetCurrentProcess();
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_ProcessId")}: {process.Id}");
            
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_AvailableWorkerThreads")}: {workerThreads}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_AvailableCompletionPortThreads")}: {completionPortThreads}");
            
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_MaxWorkerThreads")}: {workerThreads}");
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_MaxCompletionPortThreads")}: {completionPortThreads}");
#endif
        }
        catch (Exception ex)
        {
            info.AppendLine($"{GetLocalizedString("ExceptionDialog_ThreadInfoUnavailable")}: {ex.Message}");
        }
        
        return info.ToString();
    }

    private string BuildFullReport()
    {
        var report = new StringBuilder();
        
        report.AppendLine($"=== {GetLocalizedString("ExceptionDialog_ReportTitle")} ===");
        report.AppendLine($"{GetLocalizedString("ExceptionDialog_ReportCreated")}: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        
        report.AppendLine($"--- {GetLocalizedString("ExceptionDialog_ErrorMessage")} ---");
        report.AppendLine(ErrorText);
        report.AppendLine();
        
        report.AppendLine($"--- {GetLocalizedString("ExceptionDialog_ExceptionDetails")} ---");
        report.AppendLine(ExceptionDetailsText);
        report.AppendLine();
        
        report.AppendLine($"--- {GetLocalizedString("ExceptionDialog_SystemInformation")} ---");
        report.AppendLine(SystemInfoText);
        report.AppendLine();
        
        if (ShowDebugInfo == Visibility.Visible)
        {
            report.AppendLine($"--- {GetLocalizedString("ExceptionDialog_MemoryInformation")} ---");
            report.AppendLine(MemoryInfoText);
            report.AppendLine();
            
            report.AppendLine($"--- {GetLocalizedString("ExceptionDialog_ThreadInformation")} ---");
            report.AppendLine(ThreadInfoText);
            report.AppendLine();
        }
        
        return report.ToString();
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_fullReport);
            dataPackage.Properties.Description = GetLocalizedString("ExceptionDialog_ReportDescription");
            Clipboard.SetContent(dataPackage);
            
            // Show success feedback
            FeedbackMessage = GetLocalizedString("ExceptionDialog_CopySuccess");
        }
        catch (Exception ex)
        {
            FeedbackMessage = $"{GetLocalizedString("ExceptionDialog_CopyError")}: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveToFileAsync()
    {
        try
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = $"{GetLocalizedString("ExceptionDialog_ReportFilename")}_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            
            savePicker.FileTypeChoices.Add(GetLocalizedString("ExceptionDialog_TextFiles"), new[] { ".txt" });
            savePicker.FileTypeChoices.Add(GetLocalizedString("ExceptionDialog_LogFiles"), new[] { ".log" });

            // Get window handle for WinUI 3
            var mainWindow = ((App)Microsoft.UI.Xaml.Application.Current).MainWindow;
            if (mainWindow != null)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker,
                    WinRT.Interop.WindowNative.GetWindowHandle(mainWindow));
            }

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, _fullReport);
                FeedbackMessage = $"{GetLocalizedString("ExceptionDialog_SaveSuccess")} {file.Name}.";
            }
        }
        catch (Exception ex)
        {
            FeedbackMessage = $"{GetLocalizedString("ExceptionDialog_SaveError")}: {ex.Message}";
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:N2} {suffixes[suffixIndex]}";
    }
}