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

        /// <summary>
        /// Ceils a duration to the next minute. If the duration is already an exact minute, it remains unchanged.
        /// </summary>
        /// <param name="duration">The duration to ceil.</param>
        /// <returns>Duration ceiled to the next minute.</returns>
        public static TimeSpan CeilToNextMinute(TimeSpan duration)
        {
            if (duration.TotalMinutes == Math.Floor(duration.TotalMinutes))
            {
                // Duration is already an exact minute
                return duration;
            }
            
            // Ceil to next minute
            return TimeSpan.FromMinutes(Math.Ceiling(duration.TotalMinutes));
        }

        /// <summary>
        /// Ceils a DateTime to the next minute. If the time is already at exact minute (seconds = 0), it remains unchanged.
        /// </summary>
        /// <param name="dateTime">The DateTime to ceil.</param>
        /// <returns>DateTime ceiled to the next minute.</returns>
        public static DateTime CeilDateTimeToNextMinute(DateTime dateTime)
        {
            if (dateTime.Second == 0 && dateTime.Millisecond == 0)
            {
                // Already at exact minute
                return dateTime;
            }
            
            // Ceil to next minute by adding 1 minute and zeroing seconds/milliseconds
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 
                dateTime.Hour, dateTime.Minute, 0, 0).AddMinutes(1);
        }

        /// <summary>
        /// Calculates the duration in minutes between two ceiled timestamps.
        /// </summary>
        /// <param name="startTime">Start time to be ceiled.</param>
        /// <param name="endTime">End time to be ceiled.</param>
        /// <returns>Duration in minutes between the ceiled timestamps.</returns>
        public static int CalculateCeiledDurationInMinutes(DateTime startTime, DateTime endTime)
        {
            if (endTime < startTime)
            {
                throw new InvalidOperationException("End time cannot be before start time.");
            }

            var ceiledStartTime = CeilDateTimeToNextMinute(startTime);
            var ceiledEndTime = CeilDateTimeToNextMinute(endTime);
            
            if (ceiledEndTime <= ceiledStartTime)
            {
                return 0; // Duration becomes zero after ceiling
            }

            var duration = ceiledEndTime.Subtract(ceiledStartTime);
            return (int)duration.TotalMinutes;
        }

        /// <summary>
        /// Calculates the duration in minutes for a desktop usage entry using ceiled timestamps.
        /// </summary>
        /// <param name="entry">The desktop usage entry.</param>
        /// <returns>Duration in minutes between the ceiled timestamps.</returns>
        public static int CalculateCeiledDurationInMinutes(DesktopUsageEntry entry)
        {
            var endTime = entry.EndTime ?? DateTime.Now;
            return CalculateCeiledDurationInMinutes(entry.StartTime, endTime);
        }

        /// <summary>
        /// Gets the ceiled start and end times for a desktop usage entry.
        /// </summary>
        /// <param name="entry">The desktop usage entry.</param>
        /// <returns>Tuple containing ceiled start time and ceiled end time.</returns>
        public static (DateTime CeiledStartTime, DateTime CeiledEndTime) GetCeiledTimes(DesktopUsageEntry entry)
        {
            var endTime = entry.EndTime ?? DateTime.Now;
            return (CeilDateTimeToNextMinute(entry.StartTime), CeilDateTimeToNextMinute(endTime));
        }

        /// <summary>
        /// Filters out entries that have zero duration after ceiling to minutes.
        /// </summary>
        /// <param name="entries">The entries to filter.</param>
        /// <returns>Entries with non-zero ceiled duration in minutes.</returns>
        public static List<DesktopUsageEntry> FilterZeroDurationEntries(List<DesktopUsageEntry> entries)
        {
            return entries.Where(entry => CalculateCeiledDurationInMinutes(entry) > 0).ToList();
        }
    }
}
