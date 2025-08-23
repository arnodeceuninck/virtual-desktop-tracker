using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace VirtualDesktopDisplayer.Services
{
    /// <summary>
    /// Service for handling application and file operations.
    /// </summary>
    public class ApplicationService
    {
        /// <summary>
        /// Opens a file in Notepad.
        /// </summary>
        /// <param name="filePath">Path to the file to open.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool OpenFileInNotepad(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening file in Notepad: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens a folder in Windows Explorer.
        /// </summary>
        /// <param name="folderPath">Path to the folder to open.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool OpenFolderInExplorer(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return false;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{folderPath}\"",
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening folder: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shows an information message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The title of the message box.</param>
        public void ShowInformation(string message, string title = "Virtual Desktop Tracker")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <param name="title">The title of the message box.</param>
        public void ShowError(string message, string title = "Virtual Desktop Tracker")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a warning message box.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        /// <param name="title">The title of the message box.</param>
        public void ShowWarning(string message, string title = "Rename Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void ExitApplication()
        {
            Application.Exit();
        }
    }
}
