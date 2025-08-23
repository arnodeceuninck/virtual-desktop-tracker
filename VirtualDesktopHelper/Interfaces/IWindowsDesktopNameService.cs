namespace VirtualDesktopHelper.Interfaces
{
    /// <summary>
    /// Provides methods to interact with Windows virtual desktop names.
    /// </summary>
    public interface IWindowsDesktopNameService
    {
        /// <summary>
        /// Gets the name of the currently active virtual desktop.
        /// </summary>
        /// <returns>The name of the current desktop, or an error message if unavailable.</returns>
        string GetCurrentDesktopName();

        /// <summary>
        /// Renames the currently active virtual desktop.
        /// </summary>
        /// <param name="newName">The new name for the desktop.</param>
        /// <returns>True if rename was successful, false otherwise.</returns>
        bool RenameCurrentDesktop(string newName);
    }
}
