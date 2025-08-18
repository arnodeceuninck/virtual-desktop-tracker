using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualDesktopHelper;

namespace VirtualDesktopDisplayer
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer updateTimer;
        private Label desktopLabel;
        private const int TASKBAR_HEIGHT = 40; // Approximate taskbar height
        private const int MARGIN = 10;

        public Form1()
        {
            InitializeComponent();
            InitializeDesktopDisplay();
            SetupTimer();
            PositionWindow();
        }

        private void InitializeDesktopDisplay()
        {
            // Configure the form
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;

            // Create and configure the label
            desktopLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(128, 0, 0, 0), // Semi-transparent black
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(8, 4, 8, 4)
            };

            this.Controls.Add(desktopLabel);
            
            // Add event handlers
            this.DoubleClick += Form1_DoubleClick;
            this.MouseClick += Form1_MouseClick;
            desktopLabel.DoubleClick += Form1_DoubleClick;
            desktopLabel.MouseClick += Form1_MouseClick;
            
            UpdateDesktopName();
        }

        private void SetupTimer()
        {
            updateTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // Update every second
            };
            updateTimer.Tick += (sender, e) => UpdateDesktopName();
            updateTimer.Start();
        }

        private void UpdateDesktopName()
        {
            try
            {
                string desktopName = DesktopNameProvider.GetCurrentDesktopNameUsingSubprocess();
                desktopLabel.Text = $"Desktop: {desktopName}";
                
                // Resize form to fit the label
                this.Size = desktopLabel.PreferredSize;
                PositionWindow();
            }
            catch (Exception ex)
            {
                desktopLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void PositionWindow()
        {
            // Position in bottom right corner, above the taskbar
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                workingArea.Right - this.Width - MARGIN,
                workingArea.Bottom - this.Height - MARGIN
            );
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Make sure the window stays positioned correctly
            PositionWindow();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            base.OnFormClosing(e);
        }

        // Handle double-click to close the application
        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Handle right-click to show context menu
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Exit", null, (s, args) => Application.Exit());
                contextMenu.Show(this, e.Location);
            }
        }
    }
}
