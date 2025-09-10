using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Form for configuring issue tracking integration settings.
    /// </summary>
    public partial class IssueTrackingConfigurationForm : Form
    {
        private readonly TrackerConfiguration _config;
        private readonly IssueTrackingService _issueService;
        
        // UI Controls
        private CheckBox _enableIssueTrackingCheckBox;
        private Label _regexLabel;
        private TextBox _regexTextBox;
        private Label _regexExampleLabel;
        private Label _urlTemplateLabel;
        private TextBox _urlTemplateTextBox;
        private Label _urlTemplateExampleLabel;
        private Button _testButton;
        private Label _testResultLabel;
        private TextBox _testDesktopNameTextBox;
        private Button _saveButton;
        private Button _cancelButton;

        public IssueTrackingConfigurationForm()
        {
            _config = TrackerConfiguration.Instance;
            _issueService = new IssueTrackingService(_config);
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Issue Tracking Configuration";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(10),
                AutoSize = true
            };

            // Enable issue tracking checkbox
            _enableIssueTrackingCheckBox = new CheckBox
            {
                Text = "Enable issue tracking integration",
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            _enableIssueTrackingCheckBox.CheckedChanged += OnEnableIssueTrackingChanged;

            // Issue format regex
            _regexLabel = new Label
            {
                Text = "Issue Format (Regular Expression):",
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            _regexTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = @"\b[A-Z][A-Z0-9]+-\d+\b"
            };
            _regexTextBox.TextChanged += OnRegexTextChanged;

            _regexExampleLabel = new Label
            {
                Text = "Example: \\b[A-Z][A-Z0-9]+-\\d+\\b matches APP-5482, PROJ-123, etc.",
                ForeColor = Color.Gray,
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            // Issue URL template
            _urlTemplateLabel = new Label
            {
                Text = "Issue URL Template:",
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            _urlTemplateTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "https://www.issuetracker.com/browse/{0}"
            };
            _urlTemplateTextBox.TextChanged += OnUrlTemplateTextChanged;

            _urlTemplateExampleLabel = new Label
            {
                Text = "Use {0} as placeholder for the issue identifier. Example: https://jira.company.com/browse/{0}",
                ForeColor = Color.Gray,
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            // Test section
            var testPanel = new GroupBox
            {
                Text = "Test Configuration",
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var testLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(5),
                AutoSize = true
            };

            var testLabel = new Label
            {
                Text = "Test with desktop name:",
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            _testDesktopNameTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Working on APP-5482 bug fix"
            };

            _testButton = new Button
            {
                Text = "Test",
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            _testButton.Click += OnTestButtonClick;

            _testResultLabel = new Label
            {
                Text = "",
                ForeColor = Color.Blue,
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            testLayout.Controls.Add(testLabel, 0, 0);
            testLayout.Controls.Add(_testDesktopNameTextBox, 0, 1);
            testLayout.Controls.Add(_testButton, 0, 2);
            testLayout.Controls.Add(_testResultLabel, 0, 3);
            testPanel.Controls.Add(testLayout);

            // Buttons
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true
            };

            _saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            _saveButton.Click += OnSaveButtonClick;

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            buttonPanel.Controls.Add(_saveButton, 0, 0);
            buttonPanel.Controls.Add(_cancelButton, 1, 0);

            // Add all controls to main panel
            mainPanel.Controls.Add(_enableIssueTrackingCheckBox, 0, 0);
            mainPanel.Controls.Add(_regexLabel, 0, 1);
            mainPanel.Controls.Add(_regexTextBox, 0, 2);
            mainPanel.Controls.Add(_regexExampleLabel, 0, 3);
            mainPanel.Controls.Add(_urlTemplateLabel, 0, 4);
            mainPanel.Controls.Add(_urlTemplateTextBox, 0, 5);
            mainPanel.Controls.Add(_urlTemplateExampleLabel, 0, 6);
            mainPanel.Controls.Add(testPanel, 0, 7);
            mainPanel.Controls.Add(buttonPanel, 0, 8);

            // Configure row styles to prevent excessive spacing
            for (int i = 0; i < mainPanel.RowCount; i++)
            {
                if (i == 7) // Test panel should be larger
                {
                    mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
                }
                else if (i == 8) // Button panel
                {
                    mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }
                else
                {
                    mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                }
            }

            this.Controls.Add(mainPanel);
            this.AcceptButton = _saveButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadCurrentSettings()
        {
            _enableIssueTrackingCheckBox.Checked = _config.EnableIssueTracking;
            _regexTextBox.Text = _config.IssueFormatRegex;
            _urlTemplateTextBox.Text = _config.IssueUrlTemplate;
            
            UpdateControlStates();
        }

        private void OnEnableIssueTrackingChanged(object? sender, EventArgs e)
        {
            UpdateControlStates();
        }

        private void UpdateControlStates()
        {
            bool enabled = _enableIssueTrackingCheckBox.Checked;
            
            _regexTextBox.Enabled = enabled;
            _urlTemplateTextBox.Enabled = enabled;
            _testDesktopNameTextBox.Enabled = enabled;
            _testButton.Enabled = enabled;
            
            if (!enabled)
            {
                _testResultLabel.Text = "";
            }
        }

        private void OnRegexTextChanged(object? sender, EventArgs e)
        {
            // Clear test result when regex changes
            _testResultLabel.Text = "";
            
            // Validate regex
            if (!string.IsNullOrWhiteSpace(_regexTextBox.Text))
            {
                if (!IssueTrackingService.IsValidRegexPattern(_regexTextBox.Text))
                {
                    _regexTextBox.BackColor = Color.LightPink;
                }
                else
                {
                    _regexTextBox.BackColor = Color.White;
                }
            }
            else
            {
                _regexTextBox.BackColor = Color.White;
            }
        }

        private void OnUrlTemplateTextChanged(object? sender, EventArgs e)
        {
            // Clear test result when URL template changes
            _testResultLabel.Text = "";
            
            // Validate URL template
            if (!string.IsNullOrWhiteSpace(_urlTemplateTextBox.Text))
            {
                if (!IssueTrackingService.IsValidUrlTemplate(_urlTemplateTextBox.Text))
                {
                    _urlTemplateTextBox.BackColor = Color.LightPink;
                }
                else
                {
                    _urlTemplateTextBox.BackColor = Color.White;
                }
            }
            else
            {
                _urlTemplateTextBox.BackColor = Color.White;
            }
        }

        private void OnTestButtonClick(object? sender, EventArgs e)
        {
            if (!_enableIssueTrackingCheckBox.Checked)
            {
                _testResultLabel.Text = "Issue tracking is disabled.";
                _testResultLabel.ForeColor = Color.Gray;
                return;
            }

            string testDesktopName = _testDesktopNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(testDesktopName))
            {
                _testResultLabel.Text = "Please enter a desktop name to test.";
                _testResultLabel.ForeColor = Color.Orange;
                return;
            }

            // Create a temporary service with current form values
            var tempConfig = new TrackerConfiguration();
            tempConfig.EnableIssueTracking = _enableIssueTrackingCheckBox.Checked;
            tempConfig.IssueFormatRegex = _regexTextBox.Text;
            tempConfig.IssueUrlTemplate = _urlTemplateTextBox.Text;
            
            var tempService = new IssueTrackingService(tempConfig);
            
            string? issueId = tempService.ExtractIssueFromDesktopName(testDesktopName);
            
            if (issueId == null)
            {
                _testResultLabel.Text = "No issue found in desktop name.";
                _testResultLabel.ForeColor = Color.Orange;
                return;
            }

            string? issueUrl = tempService.GenerateIssueUrl(issueId);
            
            if (issueUrl == null)
            {
                _testResultLabel.Text = $"Issue found: {issueId}, but URL generation failed.";
                _testResultLabel.ForeColor = Color.Orange;
                return;
            }

            _testResultLabel.Text = $"Found: {issueId} → {issueUrl}";
            _testResultLabel.ForeColor = Color.Green;
        }

        private void OnSaveButtonClick(object? sender, EventArgs e)
        {
            // Validate settings before saving
            if (_enableIssueTrackingCheckBox.Checked)
            {
                if (string.IsNullOrWhiteSpace(_regexTextBox.Text))
                {
                    MessageBox.Show("Please enter a valid issue format regex pattern.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _regexTextBox.Focus();
                    return;
                }

                if (!IssueTrackingService.IsValidRegexPattern(_regexTextBox.Text))
                {
                    MessageBox.Show("The issue format regex pattern is invalid. Please check the syntax.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _regexTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_urlTemplateTextBox.Text))
                {
                    MessageBox.Show("Please enter a valid issue URL template.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _urlTemplateTextBox.Focus();
                    return;
                }

                if (!IssueTrackingService.IsValidUrlTemplate(_urlTemplateTextBox.Text))
                {
                    MessageBox.Show("The issue URL template is invalid. Make sure it includes {0} as placeholder and results in a valid URL.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _urlTemplateTextBox.Focus();
                    return;
                }
            }

            // Save settings
            _config.EnableIssueTracking = _enableIssueTrackingCheckBox.Checked;
            _config.IssueFormatRegex = _regexTextBox.Text.Trim();
            _config.IssueUrlTemplate = _urlTemplateTextBox.Text.Trim();
        }
    }
}
