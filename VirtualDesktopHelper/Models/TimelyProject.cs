using System.Text.Json.Serialization;

namespace VirtualDesktopHelper.Models
{
    /// <summary>
    /// Represents a Timely project from the API response.
    /// </summary>
    public class TimelyProject
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = "";

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("client")]
        public TimelyClient? Client { get; set; }

        /// <summary>
        /// Returns a formatted string for display in lists.
        /// </summary>
        public string DisplayName => $"{Name} (ID: {Id})";

        /// <summary>
        /// Returns a formatted string with client information for display.
        /// </summary>
        public string DisplayNameWithClient => Client != null 
            ? $"{Name} - {Client.Name} (ID: {Id})"
            : DisplayName;

        /// <summary>
        /// Converts this TimelyProject to a ProjectInfo for configuration.
        /// </summary>
        public Configuration.ProjectInfo ToProjectInfo()
        {
            return new Configuration.ProjectInfo
            {
                Id = Id,
                Name = Name
            };
        }

        public override string ToString() => DisplayNameWithClient;
    }

    /// <summary>
    /// Represents a Timely client from the API response.
    /// </summary>
    public class TimelyClient
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("color")]
        public string Color { get; set; } = "";

        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }
}
