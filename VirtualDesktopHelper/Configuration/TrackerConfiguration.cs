using System;

namespace VirtualDesktopHelper.Configuration
{
    /// <summary>
    /// Configuration settings for the Virtual Desktop Tracker application.
    /// </summary>
    public class TrackerConfiguration
    {
        /// <summary>
        /// How often to check for desktop changes when screen is active.
        /// </summary>
        public TimeSpan ActiveScreenUpdateInterval { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// How often to check for desktop changes when screen is off/locked.
        /// </summary>
        public TimeSpan InactiveScreenUpdateInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// How long user must be idle before considering screen "off".
        /// </summary>
        public TimeSpan ScreenOffIdleThreshold { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Margin from screen edge for display positioning (in pixels).
        /// </summary>
        public int DisplayMargin { get; set; } = 10;

        /// <summary>
        /// Approximate taskbar height for positioning calculations (in pixels).
        /// </summary>
        public int TaskbarHeight { get; set; } = 40;

        /// <summary>
        /// Directory name under Documents where logs are stored.
        /// </summary>
        public string LogDirectoryName { get; set; } = "VirtualDesktopLogs";

        /// <summary>
        /// Filename pattern for log files.
        /// </summary>
        public string LogFileNamePattern { get; set; } = "VirtualDesktopUsage_{0:yyyy-MM-dd_HH-mm-ss}.json";

        /// <summary>
        /// Filename for the usage report.
        /// </summary>
        public string ReportFileName { get; set; } = "VirtualDesktopUsageReport.txt";

        /// <summary>
        /// Maximum number of retry attempts for subprocess operations.
        /// </summary>
        public int SubprocessRetryCount { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts.
        /// </summary>
        public TimeSpan SubprocessRetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// General retry delay for error handler operations.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Timeout for subprocess operations.
        /// </summary>
        public TimeSpan SubprocessTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Name of the VirtualDesktop executable to use.
        /// </summary>
        public string VirtualDesktopExecutableName { get; set; } = "VirtualDesktop11.exe";

        /// <summary>
        /// Gets the singleton instance of the configuration.
        /// </summary>
        public static TrackerConfiguration Instance { get; } = new TrackerConfiguration();
    }
}
