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

                // Show log file location
                string logPath = DesktopUsageTracker.GetUsageLogPath();
                Console.WriteLine($"Usage log file: {logPath}");
                Console.WriteLine($"Log file exists: {File.Exists(logPath)}");
                Console.WriteLine();

                // Generate and show recent usage
                var usageLog = DesktopUsageTracker.GetUsageLog();
                Console.WriteLine($"Total usage entries: {usageLog.Count}");
                Console.WriteLine();

                if (usageLog.Count > 0)
                {
                    Console.WriteLine("Recent usage (last 10 entries):");
                    Console.WriteLine("--------------------------------");
                    
                    var recentEntries = usageLog.Count > 10 ? 
                        usageLog.GetRange(usageLog.Count - 10, 10) : 
                        usageLog;

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
                    Console.WriteLine("No usage data available yet. Start using the Virtual Desktop Displayer to begin tracking.");
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
