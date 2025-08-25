using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VirtualDesktopHelper.Models
{
    /// <summary>
    /// Represents a Timely label from the API response.
    /// </summary>
    public class TimelyLabel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("sequence")]
        public int Sequence { get; set; }

        [JsonPropertyName("parent_id")]
        public long? ParentId { get; set; }

        [JsonPropertyName("emoji")]
        public string? Emoji { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("children")]
        public List<TimelyLabel> Children { get; set; } = new List<TimelyLabel>();

        /// <summary>
        /// Gets the display name for the label.
        /// </summary>
        public string DisplayName => Name;

        /// <summary>
        /// Gets the full path of the label including parent names.
        /// </summary>
        public string FullPath { get; set; } = "";

        /// <summary>
        /// Indicates if this label is a parent/category label.
        /// </summary>
        public bool IsParent => Children.Count > 0;

        /// <summary>
        /// Indicates if this label is a child of another label.
        /// </summary>
        public bool IsChild => ParentId.HasValue;

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Represents a project-specific label configuration from the project details API.
    /// </summary>
    public class ProjectLabel
    {
        [JsonPropertyName("project_id")]
        public long ProjectId { get; set; }

        [JsonPropertyName("label_id")]
        public long LabelId { get; set; }

        [JsonPropertyName("budget")]
        public decimal? Budget { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("default")]
        public bool Default { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents detailed project information including label requirements.
    /// </summary>
    public class TimelyProjectDetails : TimelyProject
    {
        [JsonPropertyName("required_labels")]
        public bool RequiredLabels { get; set; }

        [JsonPropertyName("labels")]
        public List<ProjectLabel> Labels { get; set; } = new List<ProjectLabel>();

        [JsonPropertyName("label_ids")]
        public List<long> LabelIds { get; set; } = new List<long>();

        [JsonPropertyName("required_label_ids")]
        public List<long> RequiredLabelIds { get; set; } = new List<long>();

        [JsonPropertyName("default_label_ids")]
        public List<long> DefaultLabelIds { get; set; } = new List<long>();

        [JsonPropertyName("enable_labels")]
        public string? EnableLabels { get; set; }

        [JsonPropertyName("default_labels")]
        public bool DefaultLabels { get; set; }
    }
}
