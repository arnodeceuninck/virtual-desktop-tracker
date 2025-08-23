namespace VirtualDesktopHelper.Interfaces
{
    /// <summary>
    /// Interface for detecting screen and system state.
    /// </summary>
    public interface IScreenStateDetector
    {
        /// <summary>
        /// Determines if the screen is currently locked.
        /// </summary>
        /// <returns>True if screen is locked, false otherwise.</returns>
        bool IsScreenLocked();

        /// <summary>
        /// Determines if the screen is currently off (user has been idle for a long time).
        /// </summary>
        /// <returns>True if screen is considered off, false otherwise.</returns>
        bool IsScreenOff();

        /// <summary>
        /// Determines if the screen is either locked or off.
        /// </summary>
        /// <returns>True if screen is locked or off, false otherwise.</returns>
        bool IsScreenLockedOrOff();
    }
}
