using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Models;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Generates comprehensive usage reports from desktop usage data.
    /// </summary>
    public class UsageReportGenerator
    {
        private readonly TrackerConfiguration _config;

        public UsageReportGenerator(TrackerConfiguration? config = null)
        {
            _config = config ?? TrackerConfiguration.Instance;
        }

        /// <summary>
        /// Generates a comprehensive usage report from the provided usage entries.
        /// </summary>
        /// <param name="allEntries">All usage entries across all sessions.</param>
        /// <returns>Formatted report as a string.</returns>
        public string GenerateReport(List<DesktopUsageEntry> allEntries)
        {
            var report = new StringBuilder();
            
            BuildReportHeader(report);
            
            if (!allEntries.Any())
            {
                report.AppendLine("No usage data available.");
                return report.ToString();
            }

            var groupedByDesktop = GroupByDesktop(allEntries);
            var groupedByDate = GroupByDate(allEntries);

            BuildTotalTimeSection(report, groupedByDesktop);
            BuildDailyBreakdownSection(report, groupedByDate);

            return report.ToString();
        }

        private void BuildReportHeader(StringBuilder report)
        {
            report.AppendLine("Virtual Desktop Usage Report");
            report.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine(new string('=', 50));
            report.AppendLine();
        }

        private Dictionary<string, TimeSpan> GroupByDesktop(List<DesktopUsageEntry> allEntries)
        {
            var groupedByDesktop = new Dictionary<string, TimeSpan>();

            foreach (var entry in allEntries)
            {
                if (!groupedByDesktop.ContainsKey(entry.DesktopName))
                    groupedByDesktop[entry.DesktopName] = TimeSpan.Zero;

                groupedByDesktop[entry.DesktopName] = groupedByDesktop[entry.DesktopName].Add(entry.Duration);
            }

            return groupedByDesktop;
        }

        private Dictionary<string, List<DesktopUsageEntry>> GroupByDate(List<DesktopUsageEntry> allEntries)
        {
            var groupedByDate = new Dictionary<string, List<DesktopUsageEntry>>();

            foreach (var entry in allEntries)
            {
                string dateKey = entry.StartTime.ToString("yyyy-MM-dd");
                if (!groupedByDate.ContainsKey(dateKey))
                    groupedByDate[dateKey] = new List<DesktopUsageEntry>();

                groupedByDate[dateKey].Add(entry);
            }

            return groupedByDate;
        }

        private void BuildTotalTimeSection(StringBuilder report, Dictionary<string, TimeSpan> groupedByDesktop)
        {
            report.AppendLine("Total Time Per Desktop:");
            report.AppendLine(new string('-', 30));
            
            foreach (var kvp in groupedByDesktop.OrderByDescending(x => x.Value))
            {
                report.AppendLine($"{kvp.Key}: {FormatTimeSpan(kvp.Value)}");
            }
            report.AppendLine();
        }

        private void BuildDailyBreakdownSection(StringBuilder report, Dictionary<string, List<DesktopUsageEntry>> groupedByDate)
        {
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
