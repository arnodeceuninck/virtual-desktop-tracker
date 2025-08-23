using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Centralized error handling and retry service for virtual desktop operations.
    /// </summary>
    public class VirtualDesktopErrorHandler : IVirtualDesktopErrorHandler
    {
        private readonly TrackerConfiguration _config;
        private Exception? _lastError;
        private readonly object _lockObject = new object();
        private readonly string _logPath;

        public VirtualDesktopErrorHandler(TrackerConfiguration config)
        {
            _config = config;
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                config.LogDirectoryName
            );
            _logPath = Path.Combine(logDirectory, "errors.log");
            EnsureLogDirectoryExists();
        }

        public void LogError(Exception exception, string context, object? additionalData = null)
        {
            lock (_lockObject)
            {
                _lastError = exception;
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logEntry = $"[{timestamp}] ERROR [{context}]: {exception.Message}";
                
                if (exception.StackTrace != null)
                {
                    logEntry += Environment.NewLine + $"Stack Trace: {exception.StackTrace}";
                }
                
                if (additionalData != null)
                {
                    logEntry += Environment.NewLine + $"Additional Data: {additionalData}";
                }

                WriteToLog(logEntry);
            }
        }

        public void LogWarning(string message, string context)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logEntry = $"[{timestamp}] WARNING [{context}]: {message}";
            WriteToLog(logEntry);
        }

        public void LogInfo(string message, string context)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logEntry = $"[{timestamp}] INFO [{context}]: {message}";
            WriteToLog(logEntry);
        }

        public T ExecuteWithRetry<T>(Func<T> action, string operationName, int maxRetries = 3, TimeSpan? delay = null)
        {
            var retryDelay = delay ?? TimeSpan.FromMilliseconds(_config.RetryDelayMs);
            var attempts = 0;
            Exception? lastException = null;

            while (attempts <= maxRetries)
            {
                try
                {
                    attempts++;
                    LogInfo($"Attempting operation '{operationName}' (attempt {attempts}/{maxRetries + 1})", "RetryManager");
                    
                    var result = action();
                    
                    if (attempts > 1)
                    {
                        LogInfo($"Operation '{operationName}' succeeded on attempt {attempts}", "RetryManager");
                    }
                    
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogError(ex, $"RetryManager.{operationName}.Attempt{attempts}", new { Attempt = attempts, MaxRetries = maxRetries });

                    if (attempts <= maxRetries)
                    {
                        LogWarning($"Retrying operation '{operationName}' in {retryDelay.TotalMilliseconds}ms (attempt {attempts + 1}/{maxRetries + 1})", "RetryManager");
                        Thread.Sleep(retryDelay);
                    }
                }
            }

            LogError(lastException!, $"RetryManager.{operationName}.Failed", new { TotalAttempts = attempts, MaxRetries = maxRetries });
            throw new InvalidOperationException($"Operation '{operationName}' failed after {attempts} attempts. Last error: {lastException?.Message}", lastException);
        }

        public void ExecuteWithRetry(Action action, string operationName, int maxRetries = 3, TimeSpan? delay = null)
        {
            ExecuteWithRetry(() =>
            {
                action();
                return true; // Dummy return value
            }, operationName, maxRetries, delay);
        }

        public Exception? GetLastError()
        {
            lock (_lockObject)
            {
                return _lastError;
            }
        }

        public void ClearErrors()
        {
            lock (_lockObject)
            {
                _lastError = null;
            }
        }

        private void WriteToLog(string logEntry)
        {
            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logPath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Ignore logging errors to prevent recursive issues
            }
        }

        private void EnsureLogDirectoryExists()
        {
            try
            {
                var directory = Path.GetDirectoryName(_logPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Ignore directory creation errors
            }
        }
    }
}
