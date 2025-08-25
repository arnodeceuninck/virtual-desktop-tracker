using System;
using System.Collections.Generic;
using System.Linq;
using VirtualDesktopHelper.Models;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Utility class containing common methods for processing desktop usage entries.
    /// This eliminates code duplication across TimelyApiService, TimelyJavaScriptGenerator, and other services.
    /// </summary>
    public static class DesktopUsageUtilities
    {
        /// <summary>
        /// Ensures all entries have proper end times set. Sets EndTime to current time for any entries that are still active (EndTime = null).
        /// </summary>
        /// <param name="entries">The entries to process.</param>
        /// <returns>A new list of entries with all EndTime values properly set.</returns>
        public static List<DesktopUsageEntry> EnsureEndTimesAreSet(List<DesktopUsageEntry> entries)
        {
            DateTime now = DateTime.Now;
            var processedEntries = new List<DesktopUsageEntry>();

            foreach (var entry in entries)
            {
                processedEntries.Add(new DesktopUsageEntry
                {
                    DesktopName = entry.DesktopName,
                    StartTime = entry.StartTime,
                    EndTime = entry.EndTime ?? now
                });
            }

            return processedEntries;
        }

        /// <summary>
        /// Filters entries to only include those from the current day.
        /// </summary>
        /// <param name="allEntries">All desktop usage entries.</param>
        /// <returns>Entries filtered to current day only.</returns>
        public static List<DesktopUsageEntry> FilterCurrentDayEntries(List<DesktopUsageEntry> allEntries)
        {
            var today = DateTime.Today;
            return allEntries.Where(entry => entry.StartTime.Date == today).ToList();
        }

        /// <summary>
        /// Filters entries to only include those from the specified day.
        /// </summary>
        /// <param name="allEntries">All desktop usage entries.</param>
        /// <param name="targetDate">The target date to filter to.</param>
        /// <returns>Entries filtered to the specified date only.</returns>
        public static List<DesktopUsageEntry> FilterEntriesByDate(List<DesktopUsageEntry> allEntries, DateTime targetDate)
        {
            var startOfDay = targetDate.Date;
            var endOfDay = startOfDay.AddDays(1);
            return allEntries.Where(entry => entry.StartTime >= startOfDay && entry.StartTime < endOfDay).ToList();
        }

        /// <summary>
        /// Formats a TimeSpan into a human-readable string.
        /// This provides a consistent format across all services.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format.</param>
        /// <param name="includeSeconds">Whether to include seconds in the output. Default is true.</param>
        /// <returns>Formatted time string (e.g., "2h 15m 30s", "15m 30s", "30s").</returns>
        public static string FormatTimeSpan(TimeSpan timeSpan, bool includeSeconds = true)
        {
            var hours = (int)timeSpan.TotalHours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            if (includeSeconds)
            {
                if (hours > 0)
                    return $"{hours}h {minutes}m {seconds}s";
                else if (minutes > 0)
                    return $"{minutes}m {seconds}s";
                else
                    return $"{seconds}s";
            }
            else
            {
                if (hours > 0)
                {
                    if (minutes == 0) return $"{hours}h";
                    return $"{hours}h {minutes}m";
                }
                else
                {
                    return $"{minutes}m";
                }
            }
        }

        /// <summary>
        /// Validates that all required entries have valid data for Timely submission.
        /// </summary>
        /// <param name="entries">Entries to validate.</param>
        /// <returns>List of validation error messages. Empty if all entries are valid.</returns>
        public static List<string> ValidateEntriesForTimelySubmission(List<DesktopUsageEntry> entries)
        {
            var errors = new List<string>();

            if (!entries.Any())
            {
                errors.Add("No entries provided for validation.");
                return errors;
            }

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.DesktopName))
                {
                    errors.Add($"Entry at {entry.StartTime} has empty desktop name.");
                }

                if (entry.StartTime == default)
                {
                    errors.Add($"Entry '{entry.DesktopName}' has invalid start time.");
                }

                if (entry.EndTime.HasValue && entry.EndTime.Value <= entry.StartTime)
                {
                    errors.Add($"Entry '{entry.DesktopName}' at {entry.StartTime} has end time before or equal to start time.");
                    continue; // Skip duration check as it would throw an exception
                }

                try
                {
                    if (entry.Duration.TotalSeconds <= 0)
                    {
                        errors.Add($"Entry '{entry.DesktopName}' at {entry.StartTime} has zero or negative duration.");
                    }
                }
                catch (InvalidOperationException)
                {
                    errors.Add($"Entry '{entry.DesktopName}' at {entry.StartTime} has invalid time range.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Groups entries by date for reporting purposes.
        /// </summary>
        /// <param name="entries">Entries to group.</param>
        /// <returns>Dictionary with date strings as keys and lists of entries as values.</returns>
        public static Dictionary<string, List<DesktopUsageEntry>> GroupEntriesByDate(List<DesktopUsageEntry> entries)
        {
            var grouped = new Dictionary<string, List<DesktopUsageEntry>>();

            foreach (var entry in entries)
            {
                var dateKey = entry.StartTime.ToString("yyyy-MM-dd");

                if (!grouped.ContainsKey(dateKey))
                    grouped[dateKey] = new List<DesktopUsageEntry>();

                grouped[dateKey].Add(entry);
            }

            return grouped;
        }

        /// <summary>
        /// Calculates total time spent per desktop name.
        /// </summary>
        /// <param name="entries">Entries to analyze.</param>
        /// <returns>Dictionary with desktop names as keys and total duration as values.</returns>
        public static Dictionary<string, TimeSpan> CalculateTotalTimePerDesktop(List<DesktopUsageEntry> entries)
        {
            return entries
                .GroupBy(e => e.DesktopName)
                .ToDictionary(g => g.Key, g => TimeSpan.FromTicks(g.Sum(e => e.Duration.Ticks)));
        }
    }
}
