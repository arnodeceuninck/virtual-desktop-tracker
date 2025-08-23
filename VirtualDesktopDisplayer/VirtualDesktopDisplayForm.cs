using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using VirtualDesktopDisplayer.Services;
using VirtualDesktopHelper;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Main form for displaying virtual desktop information and handling desktop operations.
    /// </summary>
    public partial class VirtualDesktopDisplayForm : Form
    {
        // Services
        private readonly IWindowsDesktopNameService _desktopNameService;
        private readonly IDesktopUsageTracker _usageTracker;
        private readonly IScreenStateDetector _screenStateDetector;
        private readonly WindowPositionService _windowPositionService;
        private readonly VirtualDesktopWindowService _virtualDesktopService;
        private readonly ApplicationService _applicationService;
        private readonly TrackerConfiguration _config;

        // UI Components
        private System.Windows.Forms.Timer? updateTimer;
        private Label? desktopLabel;
        private TextBox? renameTextBox;

        // State
        private string _lastDesktopName = "";
        private bool _isFirstRun = true;
        private bool _isRenameMode = false;

        public VirtualDesktopDisplayForm(
            IWindowsDesktopNameService? desktopNameService = null,
            IDesktopUsageTracker? usageTracker = null,
            IScreenStateDetector? screenStateDetector = null,
            TrackerConfiguration? config = null)
        {
            // Use service provider as fallback for DI
            _desktopNameService = desktopNameService ?? VirtualDesktopServiceProvider.GetDesktopNameService();
            _usageTracker = usageTracker ?? VirtualDesktopServiceProvider.GetUsageTracker();
            _screenStateDetector = screenStateDetector ?? VirtualDesktopServiceProvider.GetScreenStateDetector();
            _config = config ?? TrackerConfiguration.Instance;

            // Initialize services
            _windowPositionService = new WindowPositionService(_config);
            _virtualDesktopService = new VirtualDesktopWindowService(_config);
            _applicationService = new ApplicationService();

            try
            {
                InitializeDesktopDisplay();
                SetupUpdateTimer();
                _windowPositionService.PositionWindowBottomRight(this);
            }
            catch (Exception ex)
            {
                _applicationService.ShowError($"Error initializing form: {ex.Message}\n\nStack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (value)
            {
                // Configure window for all virtual desktops when first shown
                _virtualDesktopService.ConfigureWindowForAllDesktops(this.Handle);
            }
        }

        private void InitializeDesktopDisplay()
        {
            ConfigureForm();
            CreateControls();
            SetupEventHandlers();
            UpdateDesktopName();
        }

        private void ConfigureForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.DarkBlue;
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "Virtual Desktop Displayer";
        }

        private void CreateControls()
        {
            // Create and configure the label
            desktopLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(8, 4, 8, 4),
                Visible = true
            };

            // Create and configure the text box for renaming
            renameTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Width = 150,
                Height = 20
            };

            this.Controls.Add(desktopLabel);
            this.Controls.Add(renameTextBox);
        }

        private void SetupEventHandlers()
        {
            this.DoubleClick += OnFormDoubleClick;
            this.MouseClick += OnFormMouseClick;
            if (desktopLabel != null)
            {
                desktopLabel.DoubleClick += OnFormDoubleClick;
                desktopLabel.MouseClick += OnFormMouseClick;
            }
            if (renameTextBox != null)
            {
                renameTextBox.KeyDown += OnRenameTextBoxKeyDown;
                renameTextBox.LostFocus += OnRenameTextBoxLostFocus;
            }
        }

        private void SetupUpdateTimer()
        {
            updateTimer = new System.Windows.Forms.Timer
            {
                Interval = (int)_config.ActiveScreenUpdateInterval.TotalMilliseconds
            };
            updateTimer.Tick += OnUpdateTimerTick;
            updateTimer.Start();
        }

        private void OnUpdateTimerTick(object? sender, EventArgs e)
        {
            UpdateDesktopName();
        }

        private void UpdateDesktopName()
        {
            try
            {
                // Don't update if we're in rename mode
                if (_isRenameMode)
                    return;

                string currentDesktop = _desktopNameService.GetCurrentDesktopName();

                if (!string.IsNullOrEmpty(currentDesktop))
                {
                    UpdateDisplayWithDesktopName(currentDesktop);
                    TrackDesktopUsageIfChanged(currentDesktop);
                    UpdateTimerInterval(currentDesktop);
                }
                else
                {
                    UpdateDisplayWithUnknownDesktop();
                }

                ResizeAndRepositionWindow();
            }
            catch (Exception ex)
            {
                if (desktopLabel != null)
                    desktopLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void UpdateDisplayWithDesktopName(string desktopName)
        {
            if (desktopLabel != null)
                desktopLabel.Text = desktopName;
            this.Text = desktopName;
        }

        private void UpdateDisplayWithUnknownDesktop()
        {
            if (desktopLabel != null)
                desktopLabel.Text = "Desktop: Unknown";
            this.Text = "Virtual Desktop Displayer - Unknown";
        }

        private void TrackDesktopUsageIfChanged(string currentDesktop)
        {
            if (currentDesktop != _lastDesktopName)
            {
                if (_isFirstRun)
                {
                    _usageTracker.TrackDesktopUsage(currentDesktop);
                    _isFirstRun = false;
                }
                else if (!string.IsNullOrEmpty(_lastDesktopName))
                {
                    LogScreenStateTransitions(currentDesktop, _lastDesktopName);
                    _usageTracker.TrackDesktopUsage(currentDesktop);
                }

                _lastDesktopName = currentDesktop;
            }
        }

        private void LogScreenStateTransitions(string currentDesktop, string lastDesktop)
        {
            if (currentDesktop == "Screen Off" && lastDesktop != "Screen Off")
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] Screen locked/off detected");
            }
            else if (lastDesktop == "Screen Off" && currentDesktop != "Screen Off")
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] Screen unlocked/on detected");
            }
        }

        private void UpdateTimerInterval(string currentDesktop)
        {
            if (updateTimer == null) return;

            int newInterval = currentDesktop == "Screen Off" ? 
                (int)_config.InactiveScreenUpdateInterval.TotalMilliseconds : 
                (int)_config.ActiveScreenUpdateInterval.TotalMilliseconds;

            if (updateTimer.Interval != newInterval)
            {
                updateTimer.Interval = newInterval;
            }
        }

        private void ResizeAndRepositionWindow()
        {
            if (desktopLabel != null)
            {
                this.Size = _windowPositionService.GetOptimalFormSize(desktopLabel);
                _windowPositionService.PositionWindowBottomRight(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _windowPositionService.PositionWindowBottomRight(this);
            _virtualDesktopService.ConfigureWindowForAllDesktops(this.Handle);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void OnFormDoubleClick(object? sender, EventArgs e)
        {
            _applicationService.ExitApplication();
        }

        private void OnFormMouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowContextMenu(e.Location);
            }
            else if (e.Button == MouseButtons.Left && !_isRenameMode)
            {
                ShowRenameTextBox();
            }
        }

        private void ShowContextMenu(Point location)
        {
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add("View Usage Log", null, OnViewUsageLogClick);
            contextMenu.Items.Add("Generate Report", null, OnGenerateReportClick);
            contextMenu.Items.Add("Open Log Folder", null, OnOpenLogFolderClick);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, (s, args) => _applicationService.ExitApplication());

            contextMenu.Show(this, location);
        }

        private void ShowRenameTextBox()
        {
            if (desktopLabel == null || renameTextBox == null) return;

            _isRenameMode = true;

            desktopLabel.Visible = false;
            renameTextBox.Visible = true;
            renameTextBox.Text = desktopLabel.Text;
            renameTextBox.SelectAll();
            renameTextBox.Location = desktopLabel.Location;

            this.Size = _windowPositionService.GetOptimalFormSizeForTextBox(renameTextBox);
            _windowPositionService.PositionWindowBottomRight(this);

            renameTextBox.Focus();
        }

        private void HideRenameTextBox()
        {
            if (desktopLabel == null || renameTextBox == null) return;

            _isRenameMode = false;

            renameTextBox.Visible = false;
            desktopLabel.Visible = true;

            ResizeAndRepositionWindow();
        }

        private void OnRenameTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyDesktopRename();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideRenameTextBox();
            }
        }

        private void OnRenameTextBoxLostFocus(object? sender, EventArgs e)
        {
            HideRenameTextBox();
        }

        private void ApplyDesktopRename()
        {
            if (renameTextBox == null || desktopLabel == null) return;

            string newName = renameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newName))
            {
                bool success = _desktopNameService.RenameCurrentDesktop(newName);
                if (success)
                {
                    desktopLabel.Text = newName;
                    _lastDesktopName = newName;
                    _usageTracker.TrackDesktopUsage(newName);
                }
                else
                {
                    _applicationService.ShowWarning("Failed to rename desktop. Please try again.");
                }
            }
            HideRenameTextBox();
        }

        private void OnViewUsageLogClick(object? sender, EventArgs e)
        {
            try
            {
                string logPath = _usageTracker.GetCurrentLogFilePath();
                if (File.Exists(logPath))
                {
                    _applicationService.OpenFileInNotepad(logPath);
                }
                else
                {
                    _applicationService.ShowInformation("No usage log found yet. The log will be created as you use different virtual desktops.");
                }
            }
            catch (Exception ex)
            {
                _applicationService.ShowError($"Error opening usage log: {ex.Message}");
            }
        }

        private void OnGenerateReportClick(object? sender, EventArgs e)
        {
            try
            {
                _usageTracker.GenerateUsageReport();
                string reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                _config.ReportFileName);

                if (File.Exists(reportPath))
                {
                    _applicationService.OpenFileInNotepad(reportPath);
                }
            }
            catch (Exception ex)
            {
                _applicationService.ShowError($"Error generating report: {ex.Message}");
            }
        }

        private void OnOpenLogFolderClick(object? sender, EventArgs e)
        {
            try
            {
                string logDirectory = _usageTracker.GetLogDirectory();
                if (!_applicationService.OpenFolderInExplorer(logDirectory))
                {
                    // Fallback to Documents folder
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    _applicationService.OpenFolderInExplorer(documentsPath);
                }
            }
            catch (Exception ex)
            {
                _applicationService.ShowError($"Error opening folder: {ex.Message}");
            }
        }
    }
}
