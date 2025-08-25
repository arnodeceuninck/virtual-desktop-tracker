using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Service for generating Timely JavaScript from desktop usage data.
    /// </summary>
    public class TimelyJavaScriptGenerator
    {
        private readonly TrackerConfiguration _config;
        private readonly TimelyConfiguration _timelyConfig;
        private readonly IUsageConsolidationService _consolidationService;
        private readonly ProjectDetectionService _projectDetectionService;

        public TimelyJavaScriptGenerator(
            TrackerConfiguration? config = null, 
            TimelyConfiguration? timelyConfig = null,
            IUsageConsolidationService? consolidationService = null,
            ProjectDetectionService? projectDetectionService = null)
        {
            _config = config ?? TrackerConfiguration.Instance;
            _timelyConfig = timelyConfig ?? TimelyConfiguration.Instance;
            _consolidationService = consolidationService ?? new UsageConsolidationService(_config);
            _projectDetectionService = projectDetectionService ?? new ProjectDetectionService();
        }

        /// <summary>
        /// Generates Timely JavaScript from desktop usage entries.
        /// </summary>
        /// <param name="allEntries">All desktop usage entries.</param>
        /// <param name="currentDayOnly">If true, only includes entries from the current day.</param>
        /// <returns>JavaScript code as a string that can be executed in browser console.</returns>
        public string GenerateTimelyJavaScript(List<DesktopUsageEntry> allEntries, bool currentDayOnly = true)
        {
            if (!_timelyConfig.IsConfigured())
            {
                throw new InvalidOperationException("Timely configuration is not properly set up. Please configure the Timely settings first.");
            }

            // Ensure all entries have proper end times before processing
            var entriesWithEndTime = DesktopUsageUtilities.EnsureEndTimesAreSet(allEntries);

            // Filter entries for current day if requested
            var filteredEntries = currentDayOnly ? DesktopUsageUtilities.FilterCurrentDayEntries(entriesWithEndTime) : entriesWithEndTime;
            
            if (!filteredEntries.Any())
            {
                throw new InvalidOperationException(currentDayOnly ? "No usage data available for today." : "No usage data available.");
            }

            // Apply consolidation if enabled
            var consolidatedEntries = _config.EnableActivityConsolidation 
                ? _consolidationService.ConsolidateUsageEntries(filteredEntries)
                : filteredEntries;

            // Group consecutive activities by desktop name
            var desktopGroups = GroupConsecutiveActivities(consolidatedEntries);
            
            // Determine target date
            string targetDate = filteredEntries.FirstOrDefault()?.StartTime.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

            return GenerateJavaScriptContent(desktopGroups, targetDate);
        }

        /// <summary>
        /// Generates a temporary JSON report and converts it to Timely JavaScript.
        /// </summary>
        /// <param name="allEntries">All desktop usage entries.</param>
        /// <param name="currentDayOnly">If true, only includes entries from the current day.</param>
        /// <returns>JavaScript code as a string.</returns>
        public async Task<string> GenerateTimelyJavaScriptFromReportAsync(List<DesktopUsageEntry> allEntries, bool currentDayOnly = true)
        {
            // Ensure all entries have proper end times before processing
            var entriesWithEndTime = DesktopUsageUtilities.EnsureEndTimesAreSet(allEntries);

            // Create a temporary JSON report
            var tempJsonPath = Path.GetTempFileName();
            try
            {
                await GenerateTemporaryJsonReportAsync(entriesWithEndTime, tempJsonPath, currentDayOnly);
                
                // Use the existing Python script to convert JSON to JavaScript
                var pythonScriptPath = FindPythonScript();
                if (string.IsNullOrEmpty(pythonScriptPath))
                {
                    // Fallback to direct C# generation
                    return GenerateTimelyJavaScript(entriesWithEndTime, currentDayOnly);
                }

                return await RunPythonScriptAsync(pythonScriptPath, tempJsonPath);
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempJsonPath))
                {
                    try { File.Delete(tempJsonPath); } catch { }
                }
            }
        }

        private Dictionary<string, List<TimestampEntry>> GroupConsecutiveActivities(List<DesktopUsageEntry> entries)
        {
            var desktopGroups = new Dictionary<string, List<TimestampEntry>>();

            // First merge consecutive identical activities
            var merged = MergeConsecutiveActivities(entries);

            // Group by desktop name, not by project
            foreach (var entry in merged.OrderBy(e => e.StartTime))
            {
                var project = _projectDetectionService.DetectProjectForEntry(entry);
                string desktopName = entry.DesktopName;
                
                if (!desktopGroups.ContainsKey(desktopName))
                {
                    desktopGroups[desktopName] = new List<TimestampEntry>();
                }

                desktopGroups[desktopName].Add(new TimestampEntry
                {
                    From = entry.StartTime.ToString($"yyyy-MM-ddTHH:mm:ss.000{_timelyConfig.TimezoneOffset}"),
                    To = (entry.EndTime ?? DateTime.Now).ToString($"yyyy-MM-ddTHH:mm:ss.000{_timelyConfig.TimezoneOffset}"),
                    DurationMinutes = entry.Duration.TotalMinutes,
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    DesktopName = entry.DesktopName,
                    LabelIds = project.LabelIds
                });
            }

            return desktopGroups;
        }

        private List<DesktopUsageEntry> MergeConsecutiveActivities(List<DesktopUsageEntry> entries)
        {
            if (!entries.Any()) return entries;

            var sorted = entries.OrderBy(e => e.StartTime).ToList();
            var merged = new List<DesktopUsageEntry>();

            foreach (var entry in sorted)
            {
                if (merged.Any() && merged.Last().DesktopName == entry.DesktopName)
                {
                    // Extend the previous entry
                    var prev = merged.Last();
                    prev.EndTime = entry.EndTime;
                    // Duration is calculated automatically
                }
                else
                {
                    merged.Add(new DesktopUsageEntry
                    {
                        DesktopName = entry.DesktopName,
                        StartTime = entry.StartTime,
                        EndTime = entry.EndTime
                        // Duration is calculated automatically
                    });
                }
            }

            return merged;
        }

        private string GenerateJavaScriptContent(Dictionary<string, List<TimestampEntry>> desktopGroups, string targetDate)
        {
            var js = new StringBuilder();

            // JavaScript header
            js.AppendLine($"// Timely API requests for {targetDate}");
            js.AppendLine("// Generated automatically from VirtualDesktop usage data");
            js.AppendLine("//");
            js.AppendLine("// Instructions:");
            js.AppendLine("// 1. Open your browser's Developer Tools (F12)");
            js.AppendLine("// 2. Go to the Console tab");
            js.AppendLine($"// 3. Navigate to: {_timelyConfig.ApiBaseUrl}/{_timelyConfig.WorkspaceId}/calendar/day?date={targetDate}");
            js.AppendLine("// 4. Copy and paste the code below into the console");
            js.AppendLine("// 5. Press Enter to execute all requests");
            js.AppendLine();
            js.AppendLine($"console.log('Starting Timely data entry for {targetDate}...');");
            js.AppendLine();

            // Helper functions
            js.AppendLine("// Helper function to delay between requests");
            js.AppendLine("function delay(ms) {");
            js.AppendLine("    return new Promise(resolve => setTimeout(resolve, ms));");
            js.AppendLine("}");
            js.AppendLine();

            // Main submission function
            GenerateSubmissionFunction(js, targetDate);

            // Entry submission calls
            GenerateEntrySubmissions(js, desktopGroups);

            // Footer
            js.AppendLine();
            js.AppendLine("    console.log('\\n✅ All Timely entries submitted!');");
            js.AppendLine("    console.log('Please refresh the page to see your updated timeline.');");
            js.AppendLine("}");
            js.AppendLine();
            js.AppendLine("// Start the submission process");
            js.AppendLine("submitAllEntries();");

            return js.ToString();
        }

        private void GenerateSubmissionFunction(StringBuilder js, string targetDate)
        {
            js.AppendLine("// Function to make a Timely API request");
            js.AppendLine("async function submitTimelyEntry(projectName, projectId, timestamps, totalMinutes, labelIds = []) {");
            js.AppendLine("    const totalHours = Math.floor(totalMinutes / 60);");
            js.AppendLine("    const remainingMinutes = totalMinutes % 60;");
            js.AppendLine();
            js.AppendLine("    const payload = {");
            js.AppendLine("        \"event\": {");
            js.AppendLine($"            \"day\": \"{targetDate}\",");
            js.AppendLine("            \"note\": projectName,");
            js.AppendLine("            \"timer_state\": \"default\",");
            js.AppendLine("            \"timer_started_on\": 0,");
            js.AppendLine("            \"timer_stopped_on\": 0,");
            js.AppendLine("            \"project_id\": projectId,");
            js.AppendLine("            \"forecast_id\": null,");
            js.AppendLine("            \"label_ids\": labelIds,");
            js.AppendLine("            \"user_ids\": [],");
            js.AppendLine("            \"entry_ids\": [],");
            js.AppendLine("            \"from\": timestamps[0].from,");
            js.AppendLine("            \"to\": timestamps[timestamps.length - 1].to,");
            js.AppendLine("            \"timestamps\": timestamps,");
            js.AppendLine("            \"hours\": totalHours,");
            js.AppendLine("            \"minutes\": remainingMinutes,");
            js.AppendLine("            \"seconds\": 0,");
            js.AppendLine("            \"estimated_hours\": 0,");
            js.AppendLine("            \"estimated_minutes\": 0,");
            js.AppendLine("            \"sequence\": 1,");
            js.AppendLine("            \"billable\": false,");
            js.AppendLine("            \"context\": {");
            js.AppendLine("                \"interaction\": \"Timestamp Selection\",");
            js.AppendLine("                \"view_context\": \"Calendar\",");
            js.AppendLine("                \"memory_experience\": \"Old\",");
            js.AppendLine("                \"memory_view\": \"Timeline\",");
            js.AppendLine("                \"calendar_view\": \"Day\",");
            js.AppendLine("                \"has_timer\": false");
            js.AppendLine("            },");
            js.AppendLine("            \"state_id\": null,");
            js.AppendLine("            \"billed\": false,");
            js.AppendLine("            \"locked\": false,");
            js.AppendLine("            \"locked_reason\": null,");
            js.AppendLine("            \"external_links\": [],");
            js.AppendLine($"            \"user_id\": {_timelyConfig.UserId}");
            js.AppendLine("        }");
            js.AppendLine("    };");
            js.AppendLine();

            // Generate fetch request
            GenerateFetchRequest(js, targetDate);

            js.AppendLine("}");
            js.AppendLine();
            js.AppendLine("// Execute all requests with delays");
            js.AppendLine("async function submitAllEntries() {");
            js.AppendLine("    console.log('\\n=== TIMELY ENTRIES TO SUBMIT ===');");
        }

        private void GenerateFetchRequest(StringBuilder js, string targetDate)
        {
            js.AppendLine("    try {");
            js.AppendLine("        console.log(`Submitting: ${projectName} (${totalHours}h ${remainingMinutes}m)`);");
            js.AppendLine();
            js.AppendLine($"        const response = await fetch(\"{_timelyConfig.ApiBaseUrl}/{_timelyConfig.WorkspaceId}/hours\", {{");
            js.AppendLine("            \"headers\": {");
            js.AppendLine("                \"accept\": \"application/json\",");
            js.AppendLine("                \"accept-language\": \"en-US,en;q=0.9,nl;q=0.8\",");
            js.AppendLine("                \"cache-control\": \"no-cache\",");
            js.AppendLine("                \"content-type\": \"application/json\",");
            js.AppendLine("                \"pragma\": \"no-cache\",");
            js.AppendLine("                \"priority\": \"u=1, i\",");
            js.AppendLine("                \"sec-ch-ua\": \"\\\"Not;A=Brand\\\";v=\\\"99\\\", \\\"Microsoft Edge\\\";v=\\\"139\\\", \\\"Chromium\\\";v=\\\"139\\\"\",");
            js.AppendLine("                \"sec-ch-ua-mobile\": \"?0\",");
            js.AppendLine("                \"sec-ch-ua-platform\": \"\\\"Windows\\\"\",");
            js.AppendLine("                \"sec-fetch-dest\": \"empty\",");
            js.AppendLine("                \"sec-fetch-mode\": \"same-origin\",");
            js.AppendLine("                \"sec-fetch-site\": \"same-origin\",");
            js.AppendLine($"                \"tl-socket-id\": \"{_timelyConfig.SocketId}\",");
            js.AppendLine($"                \"x-csrf-token\": \"{_timelyConfig.CsrfToken}\",");
            js.AppendLine($"                \"cookie\": \"{_timelyConfig.CookieString}\",");
            js.AppendLine($"                \"Referer\": \"{_timelyConfig.ApiBaseUrl}/{_timelyConfig.WorkspaceId}/calendar/day?date={targetDate}&multiUserMode=false\"");
            js.AppendLine("            },");
            js.AppendLine("            \"body\": JSON.stringify(payload),");
            js.AppendLine("            \"method\": \"POST\"");
            js.AppendLine("        });");
            js.AppendLine();
            js.AppendLine("        if (response.ok) {");
            js.AppendLine("            console.log(`✅ Successfully submitted: ${projectName}`);");
            js.AppendLine("            return await response.json();");
            js.AppendLine("        } else {");
            js.AppendLine("            console.error(`❌ Failed to submit ${projectName}:`, response.status, response.statusText);");
            js.AppendLine("            return null;");
            js.AppendLine("        }");
            js.AppendLine("    } catch (error) {");
            js.AppendLine("        console.error(`❌ Error submitting ${projectName}:`, error);");
            js.AppendLine("        return null;");
            js.AppendLine("    }");
        }

        private void GenerateEntrySubmissions(StringBuilder js, Dictionary<string, List<TimestampEntry>> desktopGroups)
        {
            foreach (var kvp in desktopGroups)
            {
                string desktopName = kvp.Key;
                var timestamps = kvp.Value;
                double totalMinutes = timestamps.Sum(t => t.DurationMinutes);
                int hours = (int)(totalMinutes / 60);
                int minutes = (int)(totalMinutes % 60);

                // Get project ID and label IDs from the first timestamp entry (all should have the same project for a desktop)
                long projectId = timestamps.FirstOrDefault()?.ProjectId ?? _timelyConfig.DefaultProjectId;
                string projectName = timestamps.FirstOrDefault()?.ProjectName ?? "Unknown Project";
                var labelIds = timestamps.FirstOrDefault()?.LabelIds ?? new List<int>();

                js.AppendLine();
                js.AppendLine($"    console.log('{desktopName} (Project: {projectName}): {hours}h {minutes}m');");
                js.AppendLine();
                js.AppendLine("    await delay(1000); // Wait 1 second between requests");

                // Format timestamps for JavaScript
                var timestampObjects = timestamps.Select(ts => 
                    $"{{\"from\": \"{ts.From}\", \"to\": \"{ts.To}\", \"entry_ids\": []}}");
                string timestampsJs = "[" + string.Join(", ", timestampObjects) + "]";
                string labelIdsJs = "[" + string.Join(", ", labelIds) + "]";

                js.AppendLine("    await submitTimelyEntry(");
                js.AppendLine($"        \"{desktopName}\","); // Use desktop name as the note
                js.AppendLine($"        {projectId},");
                js.AppendLine($"        {timestampsJs},");
                js.AppendLine($"        {totalMinutes.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},");
                js.AppendLine($"        {labelIdsJs}");
                js.AppendLine("    );");
            }
        }

        private async Task GenerateTemporaryJsonReportAsync(List<DesktopUsageEntry> allEntries, string jsonPath, bool currentDayOnly)
        {
            // This is a simplified version - you might want to use the full UsageReportGenerator logic
            var filteredEntries = currentDayOnly ? DesktopUsageUtilities.FilterCurrentDayEntries(allEntries) : allEntries;
            var consolidatedEntries = _config.EnableActivityConsolidation 
                ? _consolidationService.ConsolidateUsageEntries(filteredEntries)
                : filteredEntries;

            var projectStatistics = _projectDetectionService.GetProjectStatistics(consolidatedEntries);

            var jsonReport = new
            {
                GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TotalActivities = consolidatedEntries.Count,
                ConsolidationEnabled = _config.EnableActivityConsolidation,
                ConsolidationSettings = new
                {
                    MinDurationMinutes = _config.ConsolidationMinDurationMinutes,
                    CustomMaxDurationMinutes = _config.CustomConsolidationMaxDurationMinutes,
                    EnableConsecutiveMerging = _config.EnableConsecutiveMerging,
                    EnableCustomConsolidation = _config.EnableCustomConsolidation
                },
                ProjectSummary = projectStatistics.Select(kvp => new
                {
                    ProjectId = kvp.Key.Id,
                    ProjectName = kvp.Key.Name,
                    TotalDurationMinutes = Math.Round(kvp.Value.TotalMinutes, 2),
                    TotalDurationFormatted = DesktopUsageUtilities.FormatTimeSpan(kvp.Value)
                }).ToList(),
                Activities = consolidatedEntries.Select(entry => 
                {
                    var project = _projectDetectionService.DetectProjectForEntry(entry);
                    return new
                    {
                        DesktopName = entry.DesktopName,
                        StartTime = entry.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = entry.EndTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        DurationSeconds = (int)entry.Duration.TotalSeconds,
                        DurationMinutes = Math.Round(entry.Duration.TotalMinutes, 2),
                        DurationFormatted = DesktopUsageUtilities.FormatTimeSpan(entry.Duration),
                        Date = entry.StartTime.ToString("yyyy-MM-dd"),
                        ProjectId = project.Id,
                        ProjectName = project.Name
                    };
                }).ToList()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(jsonReport, options);
            await File.WriteAllTextAsync(jsonPath, json);
        }

        private string? FindPythonScript()
        {
            // Try to find the generate_timely_js.py script
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generate_timely_js.py"),
                Path.Combine(Environment.CurrentDirectory, "generate_timely_js.py"),
                Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName ?? "", "generate_timely_js.py")
            };

            return possiblePaths.FirstOrDefault(File.Exists);
        }

        private Task<string> RunPythonScriptAsync(string scriptPath, string jsonPath)
        {
            // This would require implementing a Python process runner
            // For now, fallback to direct generation
            throw new NotImplementedException("Python script execution not implemented yet. Using direct C# generation.");
        }

        /// <summary>
        /// Helper class for timestamp entries.
        /// </summary>
        private class TimestampEntry
        {
            public string From { get; set; } = "";
            public string To { get; set; } = "";
            public double DurationMinutes { get; set; }
            public long ProjectId { get; set; }
            public string ProjectName { get; set; } = "";
            public string DesktopName { get; set; } = "";
            public List<int> LabelIds { get; set; } = new List<int>();
        }
    }
}
