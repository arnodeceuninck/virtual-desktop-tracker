using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace VirtualDesktopHelper.Configuration
{
    /// <summary>
    /// Configuration for automatic project detection based on desktop name keywords.
    /// </summary>
    public class ProjectConfiguration
    {
        private static ProjectConfiguration? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Project mappings based on keywords in desktop names.
        /// </summary>
        public List<ProjectMapping> ProjectMappings { get; set; } = new List<ProjectMapping>();

        /// <summary>
        /// Default project information when no keywords match.
        /// </summary>
        public ProjectInfo DefaultProject { get; set; } = new ProjectInfo 
        { 
            Id = 3572980, 
            Name = "Afwezig" 
        };

        /// <summary>
        /// Filename for storing project configuration.
        /// </summary>
        public static string ConfigFileName { get; set; } = "project_config.json";

        /// <summary>
        /// Gets the singleton instance of ProjectConfiguration.
        /// </summary>
        public static ProjectConfiguration Instance
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

        private ProjectConfiguration()
        {
            InitializeDefaultMappings();
        }

        private void InitializeDefaultMappings()
        {
            ProjectMappings = new List<ProjectMapping>
            {
                new ProjectMapping
                {
                    Project = new ProjectInfo { Id = 4928536, Name = "DWH - Technisch Onderhoud Selene" },
                    Keywords = new List<string> { "simon", "selene" }
                },
                new ProjectMapping
                {
                    Project = new ProjectInfo { Id = 4928536, Name = "DWH - Data Anonimisatie en Archivering" },
                    Keywords = new List<string> { "archief", "anonimisering", "obfuscation" }
                },
                new ProjectMapping
                {
                    Project = new ProjectInfo { Id = 4927456, Name = "DWH - AI/ML Technische Verbeteringen" },
                    Keywords = new List<string> { "docker", "azureml", "staatsblad" }
                }
            };
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
        private static ProjectConfiguration LoadConfiguration()
        {
            try
            {
                string configPath = GetConfigFilePath();
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ProjectConfiguration>(json);
                    if (config != null)
                    {
                        // Ensure we have default mappings if none exist
                        if (!config.ProjectMappings.Any())
                        {
                            config.InitializeDefaultMappings();
                        }
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading project configuration: {ex.Message}");
            }

            return new ProjectConfiguration();
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
                System.Diagnostics.Debug.WriteLine($"Error saving project configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Detects the appropriate project based on desktop name keywords.
        /// </summary>
        /// <param name="desktopName">The name of the virtual desktop.</param>
        /// <returns>Project information with ID and name.</returns>
        public ProjectInfo DetectProject(string desktopName)
        {
            if (string.IsNullOrWhiteSpace(desktopName))
                return DefaultProject;

            // Check each project mapping for keyword matches
            foreach (var mapping in ProjectMappings)
            {
                if (mapping.MatchesKeywords(desktopName))
                {
                    return mapping.Project;
                }
            }

            return DefaultProject;
        }

        /// <summary>
        /// Adds a new project mapping.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <param name="projectName">The project name.</param>
        /// <param name="keywords">Keywords that trigger this project.</param>
        public void AddProjectMapping(long projectId, string projectName, List<string> keywords)
        {
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = projectId, Name = projectName },
                Keywords = keywords
            };

            ProjectMappings.Add(mapping);
            SaveConfiguration();
        }

        /// <summary>
        /// Removes a project mapping by project ID.
        /// </summary>
        /// <param name="projectId">The project ID to remove.</param>
        public void RemoveProjectMapping(long projectId)
        {
            ProjectMappings.RemoveAll(m => m.Project.Id == projectId);
            SaveConfiguration();
        }

        /// <summary>
        /// Updates an existing project mapping.
        /// </summary>
        /// <param name="projectId">The project ID to update.</param>
        /// <param name="projectName">The new project name.</param>
        /// <param name="keywords">The new keywords.</param>
        public void UpdateProjectMapping(long projectId, string projectName, List<string> keywords)
        {
            var mapping = ProjectMappings.FirstOrDefault(m => m.Project.Id == projectId);
            if (mapping != null)
            {
                mapping.Project.Name = projectName;
                mapping.Keywords = keywords;
                SaveConfiguration();
            }
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
    }

    /// <summary>
    /// Represents a project with ID and name.
    /// </summary>
    public class ProjectInfo
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";

        /// <summary>
        /// Returns a string representation of the project.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} (ID: {Id})";
        }

        /// <summary>
        /// Determines equality based on both Id and Name.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is ProjectInfo other)
            {
                return Id == other.Id && Name == other.Name;
            }
            return false;
        }

        /// <summary>
        /// Gets hash code based on Id and Name.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }

    /// <summary>
    /// Represents a mapping between keywords and a project.
    /// </summary>
    public class ProjectMapping
    {
        public ProjectInfo Project { get; set; } = new ProjectInfo();
        public List<string> Keywords { get; set; } = new List<string>();

        /// <summary>
        /// Determines if any of the keywords match the given text (case insensitive).
        /// </summary>
        /// <param name="text">The text to search for keywords.</param>
        /// <returns>True if any keyword is found in the text.</returns>
        public bool MatchesKeywords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var lowerText = text.ToLowerInvariant();
            return Keywords.Any(keyword => 
                !string.IsNullOrWhiteSpace(keyword) && 
                lowerText.Contains(keyword.ToLowerInvariant())
            );
        }

        /// <summary>
        /// Returns a string representation of the mapping.
        /// </summary>
        public override string ToString()
        {
            var keywordList = string.Join(", ", Keywords);
            return $"{Project} - Keywords: [{keywordList}]";
        }
    }
}
