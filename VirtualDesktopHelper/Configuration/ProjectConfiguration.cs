using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// Private constructor for singleton pattern.
        /// </summary>
        private ProjectConfiguration()
        {
            InitializeDefaultMappings();
        }

        /// <summary>
        /// Public parameterless constructor for JSON deserialization.
        /// </summary>
        [JsonConstructor]
        public ProjectConfiguration(List<ProjectMapping>? projectMappings = null, ProjectInfo? defaultProject = null)
        {
            ProjectMappings = projectMappings ?? new List<ProjectMapping>();
            DefaultProject = defaultProject ?? new ProjectInfo 
            { 
                Id = 3572980, 
                Name = "Afwezig" 
            };

            // Ensure we have default mappings if none exist
            if (!ProjectMappings.Any())
            {
                InitializeDefaultMappings();
            }
        }

        private void InitializeDefaultMappings()
        {
            ProjectMappings = new List<ProjectMapping>
            {
                new ProjectMapping
                {
                    Project = new ProjectInfo { Id = 4928536, Name = "DWH - Technisch Onderhoud Selene" },
                    Keywords = new List<string> { "simon", "selene" },
                    Order = 0
                },
                new ProjectMapping
                {
                    Project = new ProjectInfo { Id = 4928536, Name = "DWH - Data Anonimisatie en Archivering" },
                    Keywords = new List<string> { "archief", "anonimisering", "obfuscation" },
                    Order = 1
                },
                new ProjectMapping
                {
                    Project = new ProjectInfo { Id = 4927456, Name = "DWH - AI/ML Technische Verbeteringen" },
                    Keywords = new List<string> { "docker", "azureml", "staatsblad" },
                    Order = 2
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
                    Console.WriteLine($"Loading configuration from: {configPath}");
                    string json = File.ReadAllText(configPath);
                    Console.WriteLine($"Configuration JSON length: {json.Length}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    
                    var config = JsonSerializer.Deserialize<ProjectConfiguration>(json, options);
                    if (config != null)
                    {
                        Console.WriteLine("Configuration loaded successfully");
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading project configuration: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

            // Check each project mapping for keyword matches, ordered by priority
            foreach (var mapping in ProjectMappings.OrderBy(m => m.Order))
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
        /// <param name="labelIds">Optional label IDs to apply to this project.</param>
        public void AddProjectMapping(long projectId, string projectName, List<string> keywords, List<int>? labelIds = null)
        {
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo 
                { 
                    Id = projectId, 
                    Name = projectName,
                    LabelIds = labelIds ?? new List<int>()
                },
                Keywords = keywords,
                Order = ProjectMappings.Count // Add to end by default
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
        /// <param name="labelIds">The new label IDs.</param>
        public void UpdateProjectMapping(long projectId, string projectName, List<string> keywords, List<int>? labelIds = null)
        {
            var mapping = ProjectMappings.FirstOrDefault(m => m.Project.Id == projectId);
            if (mapping != null)
            {
                mapping.Project.Name = projectName;
                mapping.Keywords = keywords;
                mapping.Project.LabelIds = labelIds ?? new List<int>();
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
        /// Optional label IDs to apply to time entries for this project.
        /// If empty or null, no labels will be applied.
        /// </summary>
        public List<int> LabelIds { get; set; } = new List<int>();

        /// <summary>
        /// Returns a string representation of the project.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} (ID: {Id})";
        }

        /// <summary>
        /// Determines equality based on Id, Name, and LabelIds.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is ProjectInfo other)
            {
                return Id == other.Id && 
                       Name == other.Name && 
                       LabelIds.SequenceEqual(other.LabelIds);
            }
            return false;
        }

        /// <summary>
        /// Gets hash code based on Id, Name, and LabelIds.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, string.Join(",", LabelIds));
        }
    }

    /// <summary>
    /// Represents a mapping between keywords and a project.
    /// </summary>
    public class ProjectMapping
    {
        public ProjectInfo Project { get; set; } = new ProjectInfo();
        public List<string> Keywords { get; set; } = new List<string>();
        public int Order { get; set; } = 0;

        /// <summary>
        /// Determines if any of the keywords match the given text (case insensitive).
        /// Uses word boundary matching to ensure keywords are matched as whole words.
        /// </summary>
        /// <param name="text">The text to search for keywords.</param>
        /// <returns>True if any keyword is found in the text.</returns>
        public bool MatchesKeywords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return Keywords.Any(keyword => 
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return false;

                // Use regex with word boundaries to match whole words only
                // \b matches word boundaries (spaces, punctuation, start/end of string)
                var pattern = $@"\b{System.Text.RegularExpressions.Regex.Escape(keyword)}\b";
                return System.Text.RegularExpressions.Regex.IsMatch(text, pattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            });
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
