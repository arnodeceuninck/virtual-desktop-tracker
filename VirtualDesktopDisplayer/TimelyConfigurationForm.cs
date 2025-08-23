using System;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Form for configuring Timely integration settings.
    /// </summary>
    public partial class TimelyConfigurationForm : Form
    {
        private TimelyConfiguration _config;
        private TextBox? txtCsrfToken;
        private TextBox? txtCookieString;
        private TextBox? txtProjectId;
        private TextBox? txtUserId;
        private TextBox? txtWorkspaceId;
        private TextBox? txtSocketId;
        private Button? btnSave;
        private Button? btnCancel;
        private Label? lblInstructions;

        public TimelyConfigurationForm()
        {
            _config = TimelyConfiguration.Instance;
            InitializeComponent();
            LoadCurrentConfiguration();
        }

        private void InitializeComponent()
        {
            this.Text = "Timely Configuration";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Instructions label
            lblInstructions = new Label
            {
                Text = "Configure Timely integration settings. You can find these values by:\n" +
                       "1. Login to Timely in your browser\n" +
                       "2. Open Developer Tools (F12) → Network tab\n" +
                       "3. Create a manual time entry and look at the request headers\n" +
                       "4. Copy the x-csrf-token and cookie values from the request",
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(560, 80),
                AutoSize = false
            };
            this.Controls.Add(lblInstructions);

            int yPos = 100;
            int labelWidth = 120;
            int textBoxWidth = 400;

            // Workspace ID
            var lblWorkspaceId = new Label
            {
                Text = "Workspace ID:",
                Location = new System.Drawing.Point(12, yPos),
                Size = new System.Drawing.Size(labelWidth, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblWorkspaceId);

            txtWorkspaceId = new TextBox
            {
                Location = new System.Drawing.Point(140, yPos),
                Size = new System.Drawing.Size(textBoxWidth, 23)
            };
            this.Controls.Add(txtWorkspaceId);

            yPos += 35;

            // Project ID
            var lblProjectId = new Label
            {
                Text = "Project ID:",
                Location = new System.Drawing.Point(12, yPos),
                Size = new System.Drawing.Size(labelWidth, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblProjectId);

            txtProjectId = new TextBox
            {
                Location = new System.Drawing.Point(140, yPos),
                Size = new System.Drawing.Size(textBoxWidth, 23)
            };
            this.Controls.Add(txtProjectId);

            yPos += 35;

            // User ID
            var lblUserId = new Label
            {
                Text = "User ID:",
                Location = new System.Drawing.Point(12, yPos),
                Size = new System.Drawing.Size(labelWidth, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblUserId);

            txtUserId = new TextBox
            {
                Location = new System.Drawing.Point(140, yPos),
                Size = new System.Drawing.Size(textBoxWidth, 23)
            };
            this.Controls.Add(txtUserId);

            yPos += 35;

            // Socket ID
            var lblSocketId = new Label
            {
                Text = "Socket ID:",
                Location = new System.Drawing.Point(12, yPos),
                Size = new System.Drawing.Size(labelWidth, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblSocketId);

            txtSocketId = new TextBox
            {
                Location = new System.Drawing.Point(140, yPos),
                Size = new System.Drawing.Size(textBoxWidth, 23)
            };
            this.Controls.Add(txtSocketId);

            yPos += 35;

            // CSRF Token
            var lblCsrfToken = new Label
            {
                Text = "CSRF Token:",
                Location = new System.Drawing.Point(12, yPos),
                Size = new System.Drawing.Size(labelWidth, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCsrfToken);

            txtCsrfToken = new TextBox
            {
                Location = new System.Drawing.Point(140, yPos),
                Size = new System.Drawing.Size(textBoxWidth, 23),
                UseSystemPasswordChar = true
            };
            this.Controls.Add(txtCsrfToken);

            yPos += 35;

            // Cookie String
            var lblCookieString = new Label
            {
                Text = "Cookie String:",
                Location = new System.Drawing.Point(12, yPos),
                Size = new System.Drawing.Size(labelWidth, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblCookieString);

            txtCookieString = new TextBox
            {
                Location = new System.Drawing.Point(140, yPos),
                Size = new System.Drawing.Size(textBoxWidth, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtCookieString);

            yPos += 75;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(385, yPos),
                Size = new System.Drawing.Size(75, 23),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(465, yPos),
                Size = new System.Drawing.Size(75, 23),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadCurrentConfiguration()
        {
            if (txtWorkspaceId != null) txtWorkspaceId.Text = _config.WorkspaceId;
            if (txtProjectId != null) txtProjectId.Text = _config.DefaultProjectId.ToString();
            if (txtUserId != null) txtUserId.Text = _config.UserId.ToString();
            if (txtSocketId != null) txtSocketId.Text = _config.SocketId;
            if (txtCsrfToken != null) txtCsrfToken.Text = _config.CsrfToken;
            if (txtCookieString != null) txtCookieString.Text = _config.CookieString;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate and save configuration
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
