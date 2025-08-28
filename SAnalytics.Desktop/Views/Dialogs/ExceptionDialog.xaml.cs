using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SAnalytics.Desktop.Views.Dialogs;

public sealed partial class ExceptionDialog : ContentDialog
{
    public string ErrorText { get; private set; }
    public string ExceptionDetailsText { get; private set; }
    public string SystemInfoText { get; private set; }
    public string MemoryInfoText { get; private set; }
    public string ThreadInfoText { get; private set; }
    public Microsoft.UI.Xaml.Visibility ShowDebugInfo { get; private set; }
    
    private readonly Exception _exception;
    private string _fullReport;

    public ExceptionDialog(Exception exception, string? userMessage = null)
    {
        this.InitializeComponent();
        _exception = exception;
        
        // Set debug mode visibility
#if DEBUG
        ShowDebugInfo = Microsoft.UI.Xaml.Visibility.Visible;
        ExceptionDetailsExpander.IsExpanded = true; // Show details by default in debug
#else
        ShowDebugInfo = Microsoft.UI.Xaml.Visibility.Collapsed;
        ExceptionDetailsExpander.IsExpanded = false;
#endif
        
        ErrorText = userMessage ?? GetUserFriendlyMessage(exception);
        ExceptionDetailsText = BuildExceptionDetails();
        SystemInfoText = BuildSystemInfo();
        MemoryInfoText = BuildMemoryInfo();
        ThreadInfoText = BuildThreadInfo();
        _fullReport = BuildFullReport();
        
        // Set up button event handlers
        this.PrimaryButtonClick += OnPrimaryButtonClick;
        this.SecondaryButtonClick += OnSecondaryButtonClick;
    }

    private string GetUserFriendlyMessage(Exception ex)
    {
#if DEBUG
        return ex.Message; // Show actual message in debug
#else
        // Generic messages for release mode for security
        return ex switch
        {
            FileNotFoundException => "Eine erforderliche Datei wurde nicht gefunden.",
            UnauthorizedAccessException => "Zugriff auf eine Ressource wurde verweigert.",
            TimeoutException => "Die Operation ist abgelaufen.",
            ArgumentException => "Ungültige Eingabe wurde bereitgestellt.",
            InvalidOperationException => "Ein Vorgang konnte nicht ausgeführt werden.",
            NotSupportedException => "Der angeforderte Vorgang wird nicht unterstützt.",
            _ => "Ein unerwarteter Fehler ist aufgetreten."
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
                details.AppendLine($"\n--- Inner Exception {level} ---");
                
            details.AppendLine($"Typ: {ex.GetType().FullName}");
            details.AppendLine($"Nachricht: {ex.Message}");
            
            if (!string.IsNullOrEmpty(ex.Source))
                details.AppendLine($"Quelle: {ex.Source}");
            
            if (ex.TargetSite != null)
                details.AppendLine($"Zielmethode: {ex.TargetSite.Name}");
            
            // Include full stack trace in debug mode
#if DEBUG
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                details.AppendLine("Stack Trace:");
                details.AppendLine(ex.StackTrace);
            }
            
            // Additional exception data
            if (ex.Data.Count > 0)
            {
                details.AppendLine("Zusätzliche Daten:");
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
                details.AppendLine("Stack Trace (Begrenzt):");
                details.AppendLine(limitedTrace);
                if (lines.Length > 3)
                    details.AppendLine("... (weitere Frames ausgeblendet)");
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
        
        info.AppendLine($"Anwendung: {entryAssembly?.GetName().Name ?? "Unbekannt"}");
        info.AppendLine($"Version: {entryAssembly?.GetName().Version?.ToString() ?? "Unbekannt"}");
        info.AppendLine($"Zeitstempel: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
#if DEBUG
        // Detailed system info in debug mode
        info.AppendLine($"Build-Konfiguration: Debug");
        info.AppendLine($"Assembly-Speicherort: {entryAssembly?.Location ?? "Unbekannt"}");
        info.AppendLine($"OS-Version: {Environment.OSVersion}");
        info.AppendLine($".NET-Version: {Environment.Version}");
        info.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        info.AppendLine($"64-bit Prozess: {Environment.Is64BitProcess}");
        info.AppendLine($"Prozessor-Anzahl: {Environment.ProcessorCount}");
        info.AppendLine($"Computername: {Environment.MachineName}");
        info.AppendLine($"Benutzername: {Environment.UserName}");
        info.AppendLine($"Arbeitsverzeichnis: {Environment.CurrentDirectory}");
#else
        // Limited system info in release mode
        info.AppendLine($"Build-Konfiguration: Release");
        info.AppendLine($"OS: {Environment.OSVersion.Platform}");
        info.AppendLine($".NET: {Environment.Version.Major}.{Environment.Version.Minor}");
        info.AppendLine($"Prozessoren: {Environment.ProcessorCount}");
#endif
        
        return info.ToString();
    }

    private string BuildMemoryInfo()
    {
        var info = new StringBuilder();
        
        try
        {
            var process = Process.GetCurrentProcess();
            
            info.AppendLine($"Arbeitsspeicher: {FormatBytes(process.WorkingSet64)}");
            info.AppendLine($"Privatspeicher: {FormatBytes(process.PrivateMemorySize64)}");
            info.AppendLine($"GC Gesamtspeicher: {FormatBytes(GC.GetTotalMemory(false))}");
            
#if DEBUG
            // Detailed memory info in debug mode
            info.AppendLine($"Peak Arbeitsspeicher: {FormatBytes(process.PeakWorkingSet64)}");
            info.AppendLine($"Virtueller Speicher: {FormatBytes(process.VirtualMemorySize64)}");
            info.AppendLine($"Peak Virtueller Speicher: {FormatBytes(process.PeakVirtualMemorySize64)}");
            info.AppendLine($"GC Gen 0 Sammlungen: {GC.CollectionCount(0)}");
            info.AppendLine($"GC Gen 1 Sammlungen: {GC.CollectionCount(1)}");
            info.AppendLine($"GC Gen 2 Sammlungen: {GC.CollectionCount(2)}");
#endif
        }
        catch (Exception ex)
        {
            info.AppendLine($"Speicherinformationen nicht verfügbar: {ex.Message}");
        }
        
        return info.ToString();
    }

    private string BuildThreadInfo()
    {
        var info = new StringBuilder();
        
        try
        {
            info.AppendLine($"Aktuelle Thread-ID: {Thread.CurrentThread.ManagedThreadId}");
            info.AppendLine($"Hintergrund-Thread: {Thread.CurrentThread.IsBackground}");
            info.AppendLine($"Thread-Pool-Thread: {Thread.CurrentThread.IsThreadPoolThread}");
            
#if DEBUG
            // Detailed thread info in debug mode
            var process = Process.GetCurrentProcess();
            info.AppendLine($"Prozess-ID: {process.Id}");
            
            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
            info.AppendLine($"Verfügbare Worker-Threads: {workerThreads}");
            info.AppendLine($"Verfügbare Completion-Port-Threads: {completionPortThreads}");
            
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            info.AppendLine($"Max Worker-Threads: {workerThreads}");
            info.AppendLine($"Max Completion-Port-Threads: {completionPortThreads}");
#endif
        }
        catch (Exception ex)
        {
            info.AppendLine($"Thread-Informationen nicht verfügbar: {ex.Message}");
        }
        
        return info.ToString();
    }

    private string BuildFullReport()
    {
        var report = new StringBuilder();
        
        report.AppendLine("=== ANWENDUNGS-FEHLERBERICHT ===");
        report.AppendLine($"Erstellt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        
        report.AppendLine("--- FEHLERMELDUNG ---");
        report.AppendLine(ErrorText);
        report.AppendLine();
        
        report.AppendLine("--- EXCEPTION-DETAILS ---");
        report.AppendLine(ExceptionDetailsText);
        report.AppendLine();
        
        report.AppendLine("--- SYSTEM-INFORMATIONEN ---");
        report.AppendLine(SystemInfoText);
        report.AppendLine();
        
        if (ShowDebugInfo == Microsoft.UI.Xaml.Visibility.Visible)
        {
            report.AppendLine("--- SPEICHER-INFORMATIONEN ---");
            report.AppendLine(MemoryInfoText);
            report.AppendLine();
            
            report.AppendLine("--- THREAD-INFORMATIONEN ---");
            report.AppendLine(ThreadInfoText);
            report.AppendLine();
        }
        
        return report.ToString();
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        CopyToClipboard();
    }

    private async void OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        await SaveToFileAsync();
    }

    private void CopyToClipboard()
    {
        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_fullReport);
            dataPackage.Properties.Description = "Anwendungs-Fehlerbericht";
            Clipboard.SetContent(dataPackage);
            
            // Show success feedback
            UserActionsBar.Message = "Fehlerdetails wurden erfolgreich in die Zwischenablage kopiert.";
        }
        catch (Exception ex)
        {
            UserActionsBar.Message = $"Fehler beim Kopieren in die Zwischenablage: {ex.Message}";
        }
    }

    private async Task SaveToFileAsync()
    {
        try
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = $"Fehlerbericht_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            
            savePicker.FileTypeChoices.Add("Text-Dateien", new[] { ".txt" });
            savePicker.FileTypeChoices.Add("Log-Dateien", new[] { ".log" });

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
                UserActionsBar.Message = $"Fehlerbericht wurde unter {file.Name} gespeichert.";
            }
        }
        catch (Exception ex)
        {
            UserActionsBar.Message = $"Fehler beim Speichern der Datei: {ex.Message}";
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