using System;
using System.Collections.Generic;
using System.Linq;
using VirtualDesktopHelper.Models;
using Config = VirtualDesktopHelper.Configuration;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Service for detecting appropriate projects based on desktop names and keywords.
    /// </summary>
    public class ProjectDetectionService
    {
        private readonly Config.ProjectConfiguration _projectConfig;

        public ProjectDetectionService(Config.ProjectConfiguration? projectConfig = null)
        {
            _projectConfig = projectConfig ?? Config.ProjectConfiguration.Instance;
        }

        /// <summary>
        /// Detects the appropriate project for a desktop usage entry.
        /// </summary>
        /// <param name="entry">The desktop usage entry.</param>
        /// <returns>Project information with ID and name.</returns>
        public Config.ProjectInfo DetectProjectForEntry(DesktopUsageEntry entry)
        {
            return _projectConfig.DetectProject(entry.DesktopName);
        }

        /// <summary>
        /// Detects projects for multiple desktop usage entries.
        /// </summary>
        /// <param name="entries">The desktop usage entries.</param>
        /// <returns>Dictionary mapping entries to their detected projects.</returns>
        public Dictionary<DesktopUsageEntry, Config.ProjectInfo> DetectProjectsForEntries(List<DesktopUsageEntry> entries)
        {
            var result = new Dictionary<DesktopUsageEntry, Config.ProjectInfo>();

            foreach (var entry in entries)
            {
                result[entry] = DetectProjectForEntry(entry);
            }

            return result;
        }

        /// <summary>
        /// Groups entries by their detected projects.
        /// </summary>
        /// <param name="entries">The desktop usage entries.</param>
        /// <returns>Dictionary mapping project info to lists of entries.</returns>
        public Dictionary<Config.ProjectInfo, List<DesktopUsageEntry>> GroupEntriesByProject(List<DesktopUsageEntry> entries)
        {
            var result = new Dictionary<Config.ProjectInfo, List<DesktopUsageEntry>>();

            foreach (var entry in entries)
            {
                var project = DetectProjectForEntry(entry);
                
                // Find existing project with same ID (to handle potential name differences)
                var existingProject = result.Keys.FirstOrDefault(p => p.Id == project.Id);
                if (existingProject != null)
                {
                    result[existingProject].Add(entry);
                }
                else
                {
                    result[project] = new List<DesktopUsageEntry> { entry };
                }
            }

            return result;
        }

        /// <summary>
        /// Gets project statistics for a list of entries.
        /// </summary>
        /// <param name="entries">The desktop usage entries.</param>
        /// <returns>Dictionary mapping project info to total duration.</returns>
        public Dictionary<Config.ProjectInfo, TimeSpan> GetProjectStatistics(List<DesktopUsageEntry> entries)
        {
            var projectGroups = GroupEntriesByProject(entries);
            var result = new Dictionary<Config.ProjectInfo, TimeSpan>();

            foreach (var kvp in projectGroups)
            {
                var totalDuration = kvp.Value.Aggregate(TimeSpan.Zero, (sum, entry) => sum.Add(entry.Duration));
                result[kvp.Key] = totalDuration;
            }

            return result;
        }

        /// <summary>
        /// Gets all configured projects.
        /// </summary>
        /// <returns>List of all configured project mappings.</returns>
        public List<Config.ProjectMapping> GetAllProjectMappings()
        {
            return _projectConfig.ProjectMappings.ToList();
        }

        /// <summary>
        /// Gets the default project.
        /// </summary>
        /// <returns>Default project information.</returns>
        public Config.ProjectInfo GetDefaultProject()
        {
            return _projectConfig.DefaultProject;
        }
    }
}