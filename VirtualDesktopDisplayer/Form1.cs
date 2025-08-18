using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                
                // Track desktop usage
                DesktopUsageTracker.TrackDesktopUsage(desktopName);
                
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
                
                // Add menu items
                contextMenu.Items.Add("View Usage Log", null, ViewUsageLog_Click);
                contextMenu.Items.Add("Generate Report", null, GenerateReport_Click);
                contextMenu.Items.Add("Open Log Folder", null, OpenLogFolder_Click);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("Exit", null, (s, args) => Application.Exit());
                
                contextMenu.Show(this, e.Location);
            }
        }

        private void ViewUsageLog_Click(object sender, EventArgs e)
        {
            try
            {
                string logPath = DesktopUsageTracker.GetUsageLogPath();
                if (File.Exists(logPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = $"\"{logPath}\"",
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("No usage log found yet. The log will be created as you use different virtual desktops.", 
                                    "Virtual Desktop Tracker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening usage log: {ex.Message}", 
                                "Virtual Desktop Tracker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                DesktopUsageTracker.GenerateUsageReport();
                string reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                                                "VirtualDesktopUsageReport.txt");
                
                if (File.Exists(reportPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = $"\"{reportPath}\"",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", 
                                "Virtual Desktop Tracker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenLogFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{documentsPath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening folder: {ex.Message}", 
                                "Virtual Desktop Tracker", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
