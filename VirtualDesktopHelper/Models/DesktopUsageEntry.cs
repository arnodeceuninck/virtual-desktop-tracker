using System;

namespace VirtualDesktopHelper.Models
{
    /// <summary>
    /// Represents a single desktop usage session with start/end times and duration calculation.
    /// </summary>
    public class DesktopUsageEntry
    {
        /// <summary>
        /// The name of the virtual desktop.
        /// </summary>
        public string DesktopName { get; set; } = "";

        /// <summary>
        /// When the user started using this desktop.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// When the user stopped using this desktop. Null if still active.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Calculated duration of desktop usage. If EndTime is null, calculates from StartTime to now.
        /// </summary>
        public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? DateTime.Now.Subtract(StartTime);

        /// <summary>
        /// Indicates whether this desktop session is currently active (EndTime is null).
        /// </summary>
        public bool IsActive => EndTime == null;
    }
}
