using System;
using System.IO;
using System.Threading;
using Windows.Storage;

namespace SAnalytics.Desktop.Core.Exceptions;

public static class ExceptionLogger
{
    private static readonly object _lock = new();
    private static string? _logFilePath;
    
    static ExceptionLogger()
    {
        try
        {
            var logsFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs");
            Directory.CreateDirectory(logsFolder);
            _logFilePath = Path.Combine(logsFolder, $"exceptions-{DateTime.Now:yyyy-MM-dd}.log");
        }
        catch
        {
            // If we can't create log file, logging will be disabled
            _logFilePath = null;
        }
    }

    public static void LogException(Exception exception, string source = "Unknown", string context = "")
    {
        try
        {
            var logEntry = BuildLogEntry(exception, source, context);
            WriteToFile(logEntry);
            
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Exception logged: {exception.Message}");
#endif
        }
        catch
        {
            // Silent failure - we can't log that logging failed without creating infinite recursion
        }
    }
    
    private static string BuildLogEntry(Exception exception, string source, string context)
    {
        var entry = new System.Text.StringBuilder();
        
        entry.AppendLine($"=== EXCEPTION LOG ENTRY ===");
        entry.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        entry.AppendLine($"Source: {source}");
        entry.AppendLine($"Context: {context}");
        entry.AppendLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        entry.AppendLine($"Thread Pool: {Thread.CurrentThread.IsThreadPoolThread}");
        entry.AppendLine();
        
        var ex = exception;
        int level = 0;
        
        while (ex != null)
        {
            if (level > 0)
                entry.AppendLine($"--- Inner Exception {level} ---");
                
            entry.AppendLine($"Type: {ex.GetType().FullName}");
            entry.AppendLine($"Message: {FilterSensitiveData(ex.Message)}");
            
            if (!string.IsNullOrEmpty(ex.Source))
                entry.AppendLine($"Source: {ex.Source}");
            
            if (ex.TargetSite != null)
                entry.AppendLine($"Target Site: {ex.TargetSite.Name}");
            
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                entry.AppendLine("Stack Trace:");
                entry.AppendLine(ex.StackTrace);
            }
            
            if (ex.Data.Count > 0)
            {
                entry.AppendLine("Additional Data:");
                foreach (System.Collections.DictionaryEntry item in ex.Data)
                {
                    entry.AppendLine($"  {item.Key}: {FilterSensitiveData(item.Value?.ToString() ?? "null")}");
                }
            }
            
            ex = ex.InnerException;
            level++;
        }
        
        entry.AppendLine("=== END EXCEPTION LOG ENTRY ===");
        entry.AppendLine();
        
        return entry.ToString();
    }
    
    private static void WriteToFile(string logEntry)
    {
        if (_logFilePath == null) return;
        
        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, logEntry);
                
                // Rotate log files if current file gets too large (>10MB)
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    RotateLogFiles();
                }
            }
            catch
            {
                // Silent failure
            }
        }
    }
    
    private static void RotateLogFiles()
    {
        try
        {
            var logsFolder = Path.GetDirectoryName(_logFilePath);
            if (logsFolder == null) return;
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var archivePath = Path.Combine(logsFolder, $"exceptions-{timestamp}.log");
            if (_logFilePath != null)
            {
                File.Move(_logFilePath, archivePath);
            }
            
            // Clean up old log files (keep only last 30 days)
            CleanupOldLogFiles(logsFolder);
        }
        catch
        {
            // Silent failure
        }
    }
    
    private static void CleanupOldLogFiles(string logsFolder)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var logFiles = Directory.GetFiles(logsFolder, "exceptions-*.log");
            
            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Silent failure
        }
    }
    
    private static string FilterSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var sensitivePatterns = new[]
        {
            "password", "secret", "key", "token", "credential", "connectionstring", "passphrase"
        };
        
        var result = input;
        foreach (var pattern in sensitivePatterns)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result,
                $@"\b{pattern}[=:]\s*\S+",
                $"{pattern}=[FILTERED]",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return result;
    }
}