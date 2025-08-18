using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace VirtualDesktopHelper
{
    public class DesktopUsageEntry
    {
        public string DesktopName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? DateTime.Now.Subtract(StartTime);
    }

    public class DesktopUsageTracker
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "VirtualDesktopLogs"
        );
        
        private static readonly string LogFilePath = Path.Combine(
            LogDirectory,
            $"VirtualDesktopUsage_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json"
        );
        
        private static List<DesktopUsageEntry> _usageLog = new List<DesktopUsageEntry>();
        private static string _currentDesktop = "";
        private static DateTime _currentDesktopStartTime = DateTime.Now;
        private static readonly object _lockObject = new object();

        static DesktopUsageTracker()
        {
            EnsureLogDirectoryExists();
            LoadUsageLog();
        }

        private static void EnsureLogDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch (Exception ex)
            {
                // If we can't create the directory, fall back to Documents folder
            }
        }

        private static void LoadUsageLog()
        {
            try
            {
                // For new session, start with empty log
                // We don't need to load existing data since each run creates a new file
                _usageLog = new List<DesktopUsageEntry>();
            }
            catch (Exception ex)
            {
                // If we can't load, start fresh
                _usageLog = new List<DesktopUsageEntry>();
            }
        }

        private static void SaveUsageLog()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(_usageLog, options);
                File.WriteAllText(LogFilePath, json);
            }
            catch (Exception ex)
            {
                // Silently fail - we don't want to break the UI
            }
        }

        public static void TrackDesktopUsage(string desktopName)
        {
            lock (_lockObject)
            {
                DateTime now = DateTime.Now;

                // If this is the same desktop, do nothing
                if (_currentDesktop == desktopName)
                    return;

                // Close the previous desktop session
                if (!string.IsNullOrEmpty(_currentDesktop))
                {
                    var lastEntry = _usageLog.Count > 0 ? _usageLog[_usageLog.Count - 1] : null;
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

                _usageLog.Add(newEntry);
                SaveUsageLog();
            }
        }

        public static List<DesktopUsageEntry> GetUsageLog()
        {
            lock (_lockObject)
            {
                return new List<DesktopUsageEntry>(_usageLog);
            }
        }

        public static string GetUsageLogPath()
        {
            return LogFilePath;
        }

        public static string GetLogDirectory()
        {
            return LogDirectory;
        }

        public static List<DesktopUsageEntry> GetAllUsageHistory()
        {
            var allEntries = new List<DesktopUsageEntry>();
            
            try
            {
                if (Directory.Exists(LogDirectory))
                {
                    var logFiles = Directory.GetFiles(LogDirectory, "VirtualDesktopUsage_*.json");
                    
                    foreach (var file in logFiles)
                    {
                        try
                        {
                            string json = File.ReadAllText(file);
                            var entries = JsonSerializer.Deserialize<List<DesktopUsageEntry>>(json);
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

        public static void GenerateUsageReport()
        {
            try
            {
                var reportPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "VirtualDesktopUsageReport.txt"
                );

                var report = new System.Text.StringBuilder();
                report.AppendLine("Virtual Desktop Usage Report");
                report.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                report.AppendLine(new string('=', 50));
                report.AppendLine();

                // Get all historical data from all log files
                var allEntries = GetAllUsageHistory();

                var groupedByDesktop = new Dictionary<string, TimeSpan>();
                var groupedByDate = new Dictionary<string, List<DesktopUsageEntry>>();

                foreach (var entry in allEntries)
                {
                    // Group by desktop name for total time
                    if (!groupedByDesktop.ContainsKey(entry.DesktopName))
                        groupedByDesktop[entry.DesktopName] = TimeSpan.Zero;
                    
                    groupedByDesktop[entry.DesktopName] = groupedByDesktop[entry.DesktopName].Add(entry.Duration);

                    // Group by date for daily breakdown
                    string dateKey = entry.StartTime.ToString("yyyy-MM-dd");
                    if (!groupedByDate.ContainsKey(dateKey))
                        groupedByDate[dateKey] = new List<DesktopUsageEntry>();
                    
                    groupedByDate[dateKey].Add(entry);
                }

                // Total time per desktop
                report.AppendLine("Total Time Per Desktop:");
                report.AppendLine(new string('-', 30));
                foreach (var kvp in groupedByDesktop.OrderByDescending(x => x.Value))
                {
                    report.AppendLine($"{kvp.Key}: {FormatTimeSpan(kvp.Value)}");
                }
                report.AppendLine();

                // Daily breakdown
                report.AppendLine("Daily Usage Breakdown:");
                report.AppendLine(new string('-', 30));
                foreach (var dateGroup in groupedByDate.OrderByDescending(x => x.Key))
                {
                    report.AppendLine($"\n{dateGroup.Key}:");
                    foreach (var entry in dateGroup.Value.OrderBy(x => x.StartTime))
                    {
                        string endTimeStr = entry.EndTime?.ToString("HH:mm:ss") ?? "ongoing";
                        report.AppendLine($"  {entry.StartTime:HH:mm:ss} - {endTimeStr} : {entry.DesktopName} ({FormatTimeSpan(entry.Duration)})");
                    }
                }

                File.WriteAllText(reportPath, report.ToString());
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            else if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes}m {ts.Seconds}s";
            else
                return $"{ts.Seconds}s";
        }
    }

    public class DesktopNameProvider
    {
        public static string GetCurrentDesktopNameUsingSubprocess()
        {
            try
            {
                // Path to the original VirtualDesktop executable
                string virtualDesktopExePath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "",
                    "VirtualDesktop",
                    "VirtualDesktop11-24H2.exe"
                );

                if (!File.Exists(virtualDesktopExePath))
                {
                    // Try alternative paths
                    string[] possiblePaths = {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe")
                    };

                    foreach (string path in possiblePaths)
                    {
                        string fullPath = Path.GetFullPath(path);
                        if (File.Exists(fullPath))
                        {
                            virtualDesktopExePath = fullPath;
                            break;
                        }
                    }

                    if (!File.Exists(virtualDesktopExePath))
                    {
                        throw new FileNotFoundException($"VirtualDesktop executable not found. Searched paths including: {virtualDesktopExePath}");
                    }
                }

                // Run the VirtualDesktop.exe /LIST command
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = virtualDesktopExePath,
                    Arguments = "/LIST",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start VirtualDesktop process");
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Parse the output to find the visible desktop
                    string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        if (trimmedLine.EndsWith("(visible)"))
                        {
                            // Remove "(visible)" suffix and return the desktop name
                            return trimmedLine.Substring(0, trimmedLine.Length - "(visible)".Length).Trim();
                        }
                    }
                }

                // Fallback: if we couldn't parse the output, return a default message
                return "Unknown Desktop";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
