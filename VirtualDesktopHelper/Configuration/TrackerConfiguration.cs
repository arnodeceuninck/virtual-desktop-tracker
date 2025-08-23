using System;

namespace VirtualDesktopHelper.Configuration
{
    /// <summary>
    /// Configuration settings for the Virtual Desktop Tracker application.
    /// </summary>
    public class TrackerConfiguration
    {
        /// <summary>
        /// How often to check for desktop changes when screen is active (in milliseconds).
        /// </summary>
        public int ActiveScreenUpdateInterval { get; set; } = 2000;

        /// <summary>
        /// How often to check for desktop changes when screen is off/locked (in milliseconds).
        /// </summary>
        public int InactiveScreenUpdateInterval { get; set; } = 10000;

        /// <summary>
        /// How long user must be idle before considering screen "off" (in milliseconds).
        /// </summary>
        public int ScreenOffIdleThreshold { get; set; } = 600000; // 10 minutes

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
        /// Delay between retry attempts (in milliseconds).
        /// </summary>
        public int SubprocessRetryDelay { get; set; } = 100;

        /// <summary>
        /// General retry delay for error handler operations (in milliseconds).
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Timeout for subprocess operations (in milliseconds).
        /// </summary>
        public int SubprocessTimeout { get; set; } = 5000;

        /// <summary>
        /// Gets the singleton instance of the configuration.
        /// </summary>
        public static TrackerConfiguration Instance { get; } = new TrackerConfiguration();
    }
}
