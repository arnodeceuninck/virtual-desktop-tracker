using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VirtualDesktopHelper.Configuration
{
    /// <summary>
    /// Configuration settings for Timely integration.
    /// Stores sensitive information in a separate config file.
    /// </summary>
    public class TimelyConfiguration
    {
        private static TimelyConfiguration? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Timely API base URL
        /// </summary>
        public string ApiBaseUrl { get; set; } = "https://app.timelyapp.com";

        /// <summary>
        /// Timely workspace ID (from URL)
        /// </summary>
        public string WorkspaceId { get; set; } = "946869";

        /// <summary>
        /// Default Timely project ID for time entries (used when no specific project is detected)
        /// </summary>
        public long DefaultProjectId { get; set; } = 3572980;

        /// <summary>
        /// Timely user ID
        /// </summary>
        public long UserId { get; set; } = 2190564;

        /// <summary>
        /// CSRF token for API requests (needs to be updated periodically)
        /// </summary>
        public string CsrfToken { get; set; } = "";

        /// <summary>
        /// Socket ID for API requests
        /// </summary>
        public string SocketId { get; set; } = "231680.3423";

        /// <summary>
        /// Cookie string for authentication (needs to be updated periodically)
        /// </summary>
        public string CookieString { get; set; } = "";

        /// <summary>
        /// Stored timezone offset for timestamps (may be overridden from curl request)
        /// If null, will be calculated dynamically based on system timezone
        /// </summary>
        private string? _timezoneOffset;

        /// <summary>
        /// Gets or sets the timezone offset for timestamps.
        /// If not explicitly set, returns the current system timezone offset (accounting for DST).
        /// </summary>
        public string TimezoneOffset 
        { 
            get => _timezoneOffset ?? GetSystemTimezoneOffset();
            set => _timezoneOffset = value;
        }

        /// <summary>
        /// Filename for storing Timely configuration
        /// </summary>
        public static string ConfigFileName { get; set; } = "timely_config.json";

        /// <summary>
        /// Gets the singleton instance of TimelyConfiguration.
        /// </summary>
        public static TimelyConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = LoadConfiguration();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the full path to the configuration file.
        /// </summary>
        public static string GetConfigFilePath()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, "VirtualDesktopLogs", ConfigFileName);
        }

        /// <summary>
        /// Loads configuration from file or creates default configuration.
        /// </summary>
        private static TimelyConfiguration LoadConfiguration()
        {
            try
            {
                string configPath = GetConfigFilePath();
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<TimelyConfiguration>(json);
                    return config ?? new TimelyConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Timely configuration: {ex.Message}");
            }

            return new TimelyConfiguration();
        }

        /// <summary>
        /// Saves the current configuration to file.
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                string configPath = GetConfigFilePath();
                string? directory = Path.GetDirectoryName(configPath);
                
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving Timely configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the essential configuration values are set.
        /// </summary>
        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(CsrfToken) && 
                   !string.IsNullOrEmpty(CookieString) &&
                   DefaultProjectId > 0 &&
                   UserId > 0;
        }

        /// <summary>
        /// Resets the instance to force reload from file.
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Gets the current system timezone offset in ISO 8601 format (e.g., "+02:00" or "-05:00").
        /// This method accounts for daylight saving time automatically.
        /// </summary>
        /// <returns>Timezone offset string in format +HH:MM or -HH:MM</returns>
        private static string GetSystemTimezoneOffset()
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var sign = offset.TotalMinutes >= 0 ? "+" : "-";
            var hours = Math.Abs(offset.Hours).ToString("D2");
            var minutes = Math.Abs(offset.Minutes).ToString("D2");
            return $"{sign}{hours}:{minutes}";
        }
    }
}
