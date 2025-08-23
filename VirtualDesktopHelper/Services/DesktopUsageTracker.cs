using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Models;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Service for tracking and persisting virtual desktop usage data.
    /// </summary>
    public class DesktopUsageTracker : IDesktopUsageTracker
    {
        private readonly TrackerConfiguration _config;
        private readonly IWindowsDesktopNameService _desktopNameService;
        private readonly IVirtualDesktopErrorHandler _errorHandler;
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private readonly List<DesktopUsageEntry> _currentSessionUsageLog;
        private readonly object _lockObject = new object();

        private string _currentDesktop = "";
        private DateTime _currentDesktopStartTime = DateTime.Now;

        // Constructor for clean service-based initialization
        public DesktopUsageTracker(TrackerConfiguration? config = null)
        {
            _config = config ?? TrackerConfiguration.Instance;
            // Use default implementations
            _desktopNameService = new WindowsDesktopNameService(
                new WindowsScreenStateDetector(),
                new VirtualDesktopErrorHandler(_config), 
                _config);
            _errorHandler = new VirtualDesktopErrorHandler(_config);
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _config.LogDirectoryName
            );
            _logFilePath = Path.Combine(
                _logDirectory,
                string.Format(_config.LogFileNamePattern, DateTime.Now)
            );
            _currentSessionUsageLog = new List<DesktopUsageEntry>();

            EnsureLogDirectoryExists();
        }

        // Enhanced constructor with dependency injection
        public DesktopUsageTracker(
            IWindowsDesktopNameService desktopNameService,
            IVirtualDesktopErrorHandler errorHandler,
            TrackerConfiguration? config = null)
        {
            _desktopNameService = desktopNameService ?? throw new ArgumentNullException(nameof(desktopNameService));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _config = config ?? TrackerConfiguration.Instance;
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _config.LogDirectoryName
            );
            _logFilePath = Path.Combine(
                _logDirectory,
                string.Format(_config.LogFileNamePattern, DateTime.Now)
            );
            _currentSessionUsageLog = new List<DesktopUsageEntry>();

            EnsureLogDirectoryExists();
        }

        public void TrackDesktopUsage(string desktopName)
        {
            if (string.IsNullOrWhiteSpace(desktopName))
                return;

            lock (_lockObject)
            {
                DateTime now = DateTime.Now;

                // If this is the same desktop, do nothing
                if (_currentDesktop == desktopName)
                    return;

                // Close the previous desktop session
                if (!string.IsNullOrEmpty(_currentDesktop))
                {
                    var lastEntry = _currentSessionUsageLog.LastOrDefault();
                    if (lastEntry != null && lastEntry.DesktopName == _currentDesktop && lastEntry.EndTime == null)
                    {
                        lastEntry.EndTime = now;
                    }
                }

                // Start tracking the new desktop
                _currentDesktop = desktopName;
                _currentDesktopStartTime = now;

                // Add new entry
                var newEntry = new DesktopUsageEntry
                {
                    DesktopName = desktopName,
                    StartTime = now,
                    EndTime = null // Still active
                };

                _currentSessionUsageLog.Add(newEntry);
                SaveCurrentSessionLog();
            }
        }

        public List<DesktopUsageEntry> GetCurrentSessionUsageLog()
        {
            lock (_lockObject)
            {
                return new List<DesktopUsageEntry>(_currentSessionUsageLog);
            }
        }

        public List<DesktopUsageEntry> GetAllUsageHistory()
        {
            var allEntries = new List<DesktopUsageEntry>();

            try
            {
                if (Directory.Exists(_logDirectory))
                {
                    var logFiles = Directory.GetFiles(_logDirectory, "VirtualDesktopUsage_*.json");

                    foreach (var file in logFiles)
                    {
                        try
                        {
                            var entries = LoadUsageLogFromFile(file);
                            if (entries != null)
                            {
                                allEntries.AddRange(entries);
                            }
                        }
                        catch
                        {
                            // Skip corrupted files
                        }
                    }
                }
            }
            catch
            {
                // Return empty list if we can't read files
            }

            return allEntries.OrderBy(e => e.StartTime).ToList();
        }

        public string GetCurrentLogFilePath()
        {
            return _logFilePath;
        }

        public string GetLogDirectory()
        {
            return _logDirectory;
        }

        public void GenerateUsageReport()
        {
            try
            {
                var reportPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    _config.ReportFileName
                );

                var reportGenerator = new UsageReportGenerator(_config);
                var allEntries = GetAllUsageHistory();
                var reportContent = reportGenerator.GenerateReport(allEntries, currentDayOnly: true);

                File.WriteAllText(reportPath, reportContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating usage report: {ex.Message}");
            }
        }

        private void EnsureLogDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating log directory: {ex.Message}");
            }
        }

        private void SaveCurrentSessionLog()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(_currentSessionUsageLog, options);
                File.WriteAllText(_logFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving usage log: {ex.Message}");
            }
        }

        private List<DesktopUsageEntry>? LoadUsageLogFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<DesktopUsageEntry>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
