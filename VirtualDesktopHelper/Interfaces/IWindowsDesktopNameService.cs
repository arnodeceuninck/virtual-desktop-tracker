using System.Collections.Generic;

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

        /// <summary>
        /// Gets the names of all virtual desktops.
        /// </summary>
        /// <returns>A list of all desktop names, with the current desktop marked.</returns>
        List<string> GetAllDesktopNames();

        /// <summary>
        /// Switches to the virtual desktop with the specified name.
        /// </summary>
        /// <param name="desktopName">The name of the desktop to switch to.</param>
        /// <returns>True if the switch was successful, false otherwise.</returns>
        bool SwitchToDesktop(string desktopName);

        /// <summary>
        /// Creates a new virtual desktop and optionally switches to it.
        /// </summary>
        /// <param name="switchToNew">Whether to switch to the newly created desktop.</param>
        /// <returns>True if the desktop was created successfully, false otherwise.</returns>
        bool CreateNewDesktop(bool switchToNew = true);

        /// <summary>
        /// Closes all virtual desktops except the current one.
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        bool CloseAllDesktopsExceptCurrent();
    }
}
