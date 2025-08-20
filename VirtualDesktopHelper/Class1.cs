using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    public class ScreenStateDetector
    {
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        private static extern uint GetIdleTime();

        [DllImport("user32.dll")]
        private static extern IntPtr OpenDesktop(string hDesktop, int Flags, bool Inherit, uint DesiredAccess);

        [DllImport("user32.dll")]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        private static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        private const uint SPI_GETSCREENSAVERRUNNING = 0x0072;

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        public static bool IsScreenLocked()
        {
            try
            {
                // Method 1: Check if screensaver is running
                bool isScreenSaverRunning = false;
                SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isScreenSaverRunning, 0);
                
                if (isScreenSaverRunning)
                    return true;

                // Method 2: Check if the current foreground window is the lock screen
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow != IntPtr.Zero)
                {
                    var className = new System.Text.StringBuilder(256);
                    GetClassName(foregroundWindow, className, className.Capacity);
                    string classNameString = className.ToString();

                    // Check for Windows lock screen class names
                    if (classNameString.Contains("LockApp") || 
                        classNameString.Contains("Windows.UI.Core.CoreWindow") ||
                        classNameString.Contains("ApplicationFrame"))
                    {
                        var windowText = new System.Text.StringBuilder(256);
                        GetWindowText(foregroundWindow, windowText, windowText.Capacity);
                        string windowTitle = windowText.ToString();
                        
                        // Additional check for lock screen window titles
                        if (string.IsNullOrEmpty(windowTitle) || 
                            windowTitle.Contains("Lock") ||
                            windowTitle.Contains("Sign in"))
                        {
                            return true;
                        }
                    }
                }

                // Method 3: Try to open the current desktop
                IntPtr hDesktop = OpenDesktop("default", 0, false, 0x0100);
                if (hDesktop == IntPtr.Zero)
                {
                    return true; // Can't access desktop, likely locked
                }
                CloseDesktop(hDesktop);

                return false;
            }
            catch
            {
                // If we can't determine the state, assume it's not locked
                return false;
            }
        }

        public static bool IsScreenOff()
        {
            try
            {
                // Check if user has been idle for a long time (indicating screen might be off)
                LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
                lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
                
                if (GetLastInputInfo(ref lastInputInfo))
                {
                    uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
                    // If idle for more than 10 minutes (600000 ms), consider screen might be off
                    // This is more conservative to avoid false positives
                    if (idleTime > 600000)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsScreenLockedOrOff()
        {
            return IsScreenLocked() || IsScreenOff();
        }
    }

    public class DesktopNameProvider
    {
        public static string GetCurrentDesktopNameUsingSubprocess()
        {
            try
            {
                // Path to the original VirtualDesktop executable
                string virtualDesktopExePath = "";

                // Try various paths to find the VirtualDesktop executable
                string[] possiblePaths = {
                    // From the workspace root
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    // Direct path from workspace root
                    @"c:\Users\ANK\repos\virtual-desktop-tracker\VirtualDesktop\VirtualDesktop11.exe",
                    // Relative from current directory
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "VirtualDesktop", "VirtualDesktop11.exe")
                };

                foreach (string path in possiblePaths)
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(path);
                        if (File.Exists(fullPath))
                        {
                            virtualDesktopExePath = fullPath;
                            break;
                        }
                    }
                    catch
                    {
                        // Skip invalid paths
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(virtualDesktopExePath) || !File.Exists(virtualDesktopExePath))
                {
                    throw new FileNotFoundException($"VirtualDesktop executable not found. Searched base directory: {AppDomain.CurrentDomain.BaseDirectory}");
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

        public static bool RenameCurrentDesktopUsingSubprocess(string newName)
        {
            try
            {
                // Path to the original VirtualDesktop executable
                string virtualDesktopExePath = "";

                // Try various paths to find the VirtualDesktop executable
                string[] possiblePaths = {
                    // From the workspace root
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    // Direct path from workspace root
                    @"c:\Users\ANK\repos\virtual-desktop-tracker\VirtualDesktop\VirtualDesktop11.exe",
                    // Relative from current directory
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "VirtualDesktop", "VirtualDesktop11.exe")
                };

                foreach (string path in possiblePaths)
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(path);
                        if (File.Exists(fullPath))
                        {
                            virtualDesktopExePath = fullPath;
                            break;
                        }
                    }
                    catch
                    {
                        // Skip invalid paths
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(virtualDesktopExePath) || !File.Exists(virtualDesktopExePath))
                {
                    throw new FileNotFoundException($"VirtualDesktop executable not found. Searched base directory: {AppDomain.CurrentDomain.BaseDirectory}");
                }

                // Use GetCurrentDesktop and Name in a single command pipeline
                ProcessStartInfo renameInfo = new ProcessStartInfo
                {
                    FileName = virtualDesktopExePath,
                    Arguments = $"/GetCurrentDesktop /Name:\"{newName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(renameInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start VirtualDesktop process to rename desktop");
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // For VirtualDesktop.exe, the exit code is the desktop number, not an error indicator
                    // We only consider it an error if there's actual error output
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new InvalidOperationException($"VirtualDesktop process error: {error}. Output: {output}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error renaming desktop: {ex.Message}");
                return false;
            }
        }
    }
}
