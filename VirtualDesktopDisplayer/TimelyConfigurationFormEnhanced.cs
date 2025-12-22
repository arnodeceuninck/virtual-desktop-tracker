using System;
using System.Drawing;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Enhanced Timely configuration form that supports parsing curl requests.
    /// </summary>
    public partial class TimelyConfigurationFormEnhanced : Form
    {
        private TimelyConfiguration _config;
        private CurlRequestParser _curlParser;
        
        // UI Components for curl method
        private TabControl? tabControl;
        private TabPage? tabCurl;
        private TabPage? tabManual;
        private TextBox? txtCurlRequest;
        private Button? btnParseCurl;
        private Label? lblCurlInstructions;
        private Label? lblCurlStatus;
        
        // UI Components for manual method
        private TextBox? txtCsrfToken;
        private TextBox? txtCookieString;
        private TextBox? txtProjectId;
        private Button? btnSelectProject;
        private TextBox? txtUserId;
        private TextBox? txtWorkspaceId;
        private TextBox? txtSocketId;
        
        // Common UI components
        private Button? btnSave;
        private Button? btnCancel;

        public TimelyConfigurationFormEnhanced()
        {
            _config = TimelyConfiguration.Instance;
            _curlParser = new CurlRequestParser();
            InitializeComponent();
            LoadCurrentConfiguration();
        }

        private void InitializeComponent()
        {
            this.Text = "Timely Configuration";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Create tab control
            tabControl = new TabControl
            {
                Location = new Point(12, 12),
                Size = new Size(660, 520),
                Dock = DockStyle.None
            };
            this.Controls.Add(tabControl);

            // Create curl tab
            CreateCurlTab();

            // Create manual tab
            CreateManualTab();

            // Create buttons
            CreateButtons();
        }

        private void CreateCurlTab()
        {
            tabCurl = new TabPage("Paste cURL Request (Recommended)")
            {
                UseVisualStyleBackColor = true
            };
            tabControl?.TabPages.Add(tabCurl);

            // Instructions
            lblCurlInstructions = new Label
            {
                Text = "Simply paste a cURL (cmd) request from your browser's Network tab:\n\n" +
                       "1. Open Timely in your browser and log in\n" +
                       "2. Open Developer Tools (F12) → Network tab\n" +
                       "3. Create a time entry manually in Timely\n" +
                       "4. Find the POST request to '/hours' in the Network tab\n" +
                       "5. Right-click → Copy → Copy as cURL\n" +
                       "6. Paste the cURL command below and click 'Parse cURL'",
                Location = new Point(12, 12),
                Size = new Size(620, 100),
                AutoSize = false
            };
            tabCurl.Controls.Add(lblCurlInstructions);

            // cURL text box
            txtCurlRequest = new TextBox
            {
                Location = new Point(12, 120),
                Size = new Size(620, 280),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9),
                PlaceholderText = "Paste your cURL request here..."
            };
            tabCurl.Controls.Add(txtCurlRequest);

            // Parse button
            btnParseCurl = new Button
            {
                Text = "Parse cURL Request",
                Location = new Point(12, 410),
                Size = new Size(150, 30)
            };
            btnParseCurl.Click += BtnParseCurl_Click;
            tabCurl.Controls.Add(btnParseCurl);

            // Status label
            lblCurlStatus = new Label
            {
                Location = new Point(300, 410),
                Size = new Size(330, 60),
                AutoSize = false,
                ForeColor = Color.Blue,
                Text = "Paste a cURL request and click 'Parse cURL Request' to extract configuration."
            };
            tabCurl.Controls.Add(lblCurlStatus);
        }

        private void CreateManualTab()
        {
            tabManual = new TabPage("Manual Configuration")
            {
                UseVisualStyleBackColor = true
            };
            tabControl?.TabPages.Add(tabManual);

            var instructions = new Label
            {
                Text = "Manually enter Timely configuration values:",
                Location = new Point(12, 12),
                Size = new Size(620, 30),
                AutoSize = false
            };
            tabManual.Controls.Add(instructions);

            int yPos = 50;
            int labelWidth = 120;
            int textBoxWidth = 400;

            // Helper method to create field
            void CreateField(string labelText, out TextBox textBox, bool isMultiline = false, bool isPassword = false)
            {
                var label = new Label
                {
                    Text = labelText,
                    Location = new Point(12, yPos),
                    Size = new Size(labelWidth, 23),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                tabManual.Controls.Add(label);

                textBox = new TextBox
                {
                    Location = new Point(140, yPos),
                    Size = new Size(textBoxWidth, isMultiline ? 60 : 23),
                    Multiline = isMultiline,
                    ScrollBars = isMultiline ? ScrollBars.Vertical : ScrollBars.None,
                    UseSystemPasswordChar = isPassword
                };
                tabManual.Controls.Add(textBox);

                yPos += isMultiline ? 75 : 35;
            }

            CreateField("Workspace ID:", out txtWorkspaceId);
            
            // Project ID with helper button
            CreateProjectIdField(yPos, labelWidth, textBoxWidth);
            yPos += 35;
            
            CreateField("User ID:", out txtUserId);
            CreateField("Socket ID:", out txtSocketId);
            CreateField("CSRF Token:", out txtCsrfToken, isPassword: true);
            CreateField("Cookie String:", out txtCookieString, isMultiline: true);
        }

        private void CreateProjectIdField(int yPos, int labelWidth, int textBoxWidth)
        {
            var label = new Label
            {
                Text = "Project ID:",
                Location = new Point(12, yPos),
                Size = new Size(labelWidth, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tabManual?.Controls.Add(label);

            txtProjectId = new TextBox
            {
                Location = new Point(140, yPos),
                Size = new Size(textBoxWidth - 90, 23)
            };
            tabManual?.Controls.Add(txtProjectId);

            btnSelectProject = new Button
            {
                Text = "Browse...",
                Location = new Point(140 + textBoxWidth - 85, yPos),
                Size = new Size(80, 25)
            };
            btnSelectProject.Click += BtnSelectProject_Click;
            tabManual?.Controls.Add(btnSelectProject);
        }

        private void CreateButtons()
        {
            // Save button
            btnSave = new Button
            {
                Text = "Save Configuration",
                Location = new Point(515, 545),
                Size = new Size(120, 30),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(425, 545),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadCurrentConfiguration()
        {
            // Load into manual tab fields
            if (txtWorkspaceId != null) txtWorkspaceId.Text = _config.WorkspaceId;
            if (txtProjectId != null) txtProjectId.Text = _config.DefaultProjectId.ToString();
            if (txtUserId != null) txtUserId.Text = _config.UserId.ToString();
            if (txtSocketId != null) txtSocketId.Text = _config.SocketId;
            if (txtCsrfToken != null) txtCsrfToken.Text = _config.CsrfToken;
            if (txtCookieString != null) txtCookieString.Text = _config.CookieString;
        }

        private void BtnParseCurl_Click(object? sender, EventArgs e)
        {
            if (txtCurlRequest == null || lblCurlStatus == null) return;

            try
            {
                string curlRequest = txtCurlRequest.Text.Trim();
                if (string.IsNullOrWhiteSpace(curlRequest))
                {
                    UpdateStatus("Please paste a cURL request first.", Color.Red);
                    return;
                }

                UpdateStatus("Parsing cURL request...", Color.Blue);

                var parsed = _curlParser.ParseCurlRequest(curlRequest);

                if (parsed.IsValid)
                {
                    // Update configuration with parsed values
                    _config.WorkspaceId = parsed.WorkspaceId;
                    _config.DefaultProjectId = parsed.ProjectId;
                    _config.UserId = parsed.UserId;
                    _config.SocketId = parsed.SocketId;
                    _config.CsrfToken = parsed.CsrfToken;
                    _config.CookieString = parsed.CookieString;
                    if (!string.IsNullOrEmpty(parsed.ApiBaseUrl))
                    {
                        _config.ApiBaseUrl = parsed.ApiBaseUrl;
                    }
                    // Set timezone offset if extracted from curl request
                    if (!string.IsNullOrEmpty(parsed.TimezoneOffset))
                    {
                        _config.TimezoneOffset = parsed.TimezoneOffset;
                    }

                    // Update manual tab with parsed values
                    LoadCurrentConfiguration();

                    UpdateStatus(
                        $"✅ Successfully parsed cURL request!\n" +
                        $"Workspace: {parsed.WorkspaceId}, Project: {parsed.ProjectId}, User: {parsed.UserId}\n" +
                        $"You can now save the configuration or switch to Manual tab to review.",
                        Color.Green);
                }
                else
                {
                    UpdateStatus($"❌ Failed to parse cURL request:\n{parsed.ErrorMessage}", Color.Red);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error parsing cURL request: {ex.Message}", Color.Red);
            }
        }

        private void BtnSelectProject_Click(object? sender, EventArgs e)
        {
            try
            {
                // Check if we have enough configuration to fetch projects
                if (string.IsNullOrEmpty(_config.CookieString) || string.IsNullOrEmpty(_config.WorkspaceId))
                {
                    MessageBox.Show(
                        "To browse projects, please first configure your Timely authentication.\n\n" +
                        "You can either:\n" +
                        "1. Use the 'Paste cURL Request' tab to extract configuration automatically, or\n" +
                        "2. Manually enter your Workspace ID and Cookie String in the fields below.\n\n" +
                        "After that, you'll be able to browse and select projects from your Timely workspace.",
                        "Configuration Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (var projectSelector = new TimelyProjectSelectorForm())
                {
                    var result = projectSelector.ShowDialog(this);
                    if (result == DialogResult.OK && projectSelector.SelectedProject != null)
                    {
                        // Update the project ID field
                        if (txtProjectId != null)
                        {
                            txtProjectId.Text = projectSelector.SelectedProject.Id.ToString();
                        }

                        // Update the internal configuration
                        _config.DefaultProjectId = projectSelector.SelectedProject.Id;

                        MessageBox.Show(
                            $"Selected project: {projectSelector.SelectedProject.DisplayNameWithClient}",
                            "Project Selected",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening project selector: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblCurlStatus != null)
            {
                lblCurlStatus.Text = message;
                lblCurlStatus.ForeColor = color;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Get values from manual tab (which might have been updated by curl parsing)
                if (txtWorkspaceId != null) _config.WorkspaceId = txtWorkspaceId.Text.Trim();

                if (txtProjectId != null && long.TryParse(txtProjectId.Text.Trim(), out long projectId))
                    _config.DefaultProjectId = projectId;
                else
                {
                    MessageBox.Show("Please enter a valid Project ID (numeric value).", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtUserId != null && long.TryParse(txtUserId.Text.Trim(), out long userId))
                    _config.UserId = userId;
                else
                {
                    MessageBox.Show("Please enter a valid User ID (numeric value).", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtSocketId != null) _config.SocketId = txtSocketId.Text.Trim();
                if (txtCsrfToken != null) _config.CsrfToken = txtCsrfToken.Text.Trim();
                if (txtCookieString != null) _config.CookieString = txtCookieString.Text.Trim();

                // Validate required fields
                if (string.IsNullOrEmpty(_config.CsrfToken) || string.IsNullOrEmpty(_config.CookieString))
                {
                    MessageBox.Show("CSRF Token and Cookie String are required for Timely integration.",
                                  "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save configuration
                _config.SaveConfiguration();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
