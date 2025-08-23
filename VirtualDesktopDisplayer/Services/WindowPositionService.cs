using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;

namespace VirtualDesktopDisplayer.Services
{
    /// <summary>
    /// Service for managing the desktop display window positioning and appearance.
    /// </summary>
    public class WindowPositionService
    {
        private readonly TrackerConfiguration _config;

        public WindowPositionService(TrackerConfiguration? config = null)
        {
            _config = config ?? TrackerConfiguration.Instance;
        }

        /// <summary>
        /// Positions the window in the bottom-right corner of the screen.
        /// </summary>
        /// <param name="form">The form to position.</param>
        public void PositionWindowBottomRight(Form form)
        {
            if (form == null) return;

            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            form.Location = new Point(
                workingArea.Right - form.Width - _config.DisplayMargin,
                workingArea.Bottom - form.Height - _config.DisplayMargin
            );
        }

        /// <summary>
        /// Calculates the optimal size for the form based on the content.
        /// </summary>
        /// <param name="label">The label containing the content.</param>
        /// <returns>The optimal size for the form.</returns>
        public Size GetOptimalFormSize(Label label)
        {
            if (label == null) return new Size(100, 30);
            return label.PreferredSize;
        }

        /// <summary>
        /// Calculates the optimal size for the form when showing a text box.
        /// </summary>
        /// <param name="textBox">The text box being displayed.</param>
        /// <returns>The optimal size for the form.</returns>
        public Size GetOptimalFormSizeForTextBox(TextBox textBox)
        {
            if (textBox == null) return new Size(150, 30);
            return new Size(textBox.Width + 16, textBox.Height + 8);
        }
    }
}
