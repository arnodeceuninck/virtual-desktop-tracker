using System;
using System.IO;
using VirtualDesktopHelper;

namespace DesktopUsageViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Virtual Desktop Usage Viewer");
            Console.WriteLine("============================");
            Console.WriteLine();

            try
            {
                // Get current desktop
                string currentDesktop = DesktopNameProvider.GetCurrentDesktopNameUsingSubprocess();
                Console.WriteLine($"Current Desktop: {currentDesktop}");
                Console.WriteLine();

                // Show log directory and current file
                string logDirectory = DesktopUsageTracker.GetLogDirectory();
                string currentLogPath = DesktopUsageTracker.GetUsageLogPath();
                Console.WriteLine($"Usage log directory: {logDirectory}");
                Console.WriteLine($"Current log file: {Path.GetFileName(currentLogPath)}");
                Console.WriteLine($"Log directory exists: {Directory.Exists(logDirectory)}");
                
                // Count log files
                int logFileCount = 0;
                if (Directory.Exists(logDirectory))
                {
                    logFileCount = Directory.GetFiles(logDirectory, "VirtualDesktopUsage_*.json").Length;
                }
                Console.WriteLine($"Total log files: {logFileCount}");
                Console.WriteLine();

                // Get all historical usage data
                var allUsageLog = DesktopUsageTracker.GetAllUsageHistory();
                Console.WriteLine($"Total usage entries across all sessions: {allUsageLog.Count}");
                Console.WriteLine();

                if (allUsageLog.Count > 0)
                {
                    Console.WriteLine("Recent usage (last 10 entries across all sessions):");
                    Console.WriteLine("---------------------------------------------------");
                    
                    var recentEntries = allUsageLog.Count > 10 ? 
                        allUsageLog.GetRange(allUsageLog.Count - 10, 10) : 
                        allUsageLog;

                    foreach (var entry in recentEntries)
                    {
                        string endTime = entry.EndTime?.ToString("HH:mm:ss") ?? "ongoing";
                        string duration = FormatTimeSpan(entry.Duration);
                        Console.WriteLine($"{entry.StartTime:yyyy-MM-dd HH:mm:ss} - {endTime} : {entry.DesktopName} ({duration})");
                    }
                    Console.WriteLine();

                    // Generate report
                    Console.WriteLine("Generating usage report...");
                    DesktopUsageTracker.GenerateUsageReport();
                    
                    string reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                                                    "VirtualDesktopUsageReport.txt");
                    Console.WriteLine($"Report generated: {reportPath}");
                }
                else
                {
                    Console.WriteLine("No usage data available yet. Start the Virtual Desktop Tracker to begin tracking.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
}
