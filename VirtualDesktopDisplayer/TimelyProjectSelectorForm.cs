using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Form for searching and selecting Timely projects.
    /// </summary>
    public partial class TimelyProjectSelectorForm : Form
    {
        private TimelyProjectService? _projectService;
        private List<TimelyProject> _allProjects = new List<TimelyProject>();
        private List<TimelyProject> _filteredProjects = new List<TimelyProject>();

        // UI Components
        private TextBox? txtSearch;
        private ListBox? lstProjects;
        private Button? btnRefresh;
        private Button? btnSelect;
        private Button? btnCancel;
        private Label? lblStatus;
        private Label? lblInstructions;
        private ProgressBar? progressBar;

        /// <summary>
        /// The selected project, or null if none is selected.
        /// </summary>
        public TimelyProject? SelectedProject { get; private set; }

        public TimelyProjectSelectorForm()
        {
            _projectService = new TimelyProjectService();
            InitializeComponent();
            LoadProjectsAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Timely Project";
            this.Size = new Size(620, 540);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Instructions
            lblInstructions = new Label
            {
                Text = "Search and select a project from your Timely workspace:",
                Location = new Point(12, 12),
                Size = new Size(560, 20),
                AutoSize = false
            };
            this.Controls.Add(lblInstructions);

            // Search box
            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(12, 40),
                Size = new Size(50, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(70, 40),
                Size = new Size(420, 23),
                PlaceholderText = "Type to search projects by name, client, or ID..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(500, 40),
                Size = new Size(70, 25)
            };
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            // Projects list
            lstProjects = new ListBox
            {
                Location = new Point(12, 75),
                Size = new Size(558, 300),
                DisplayMember = "DisplayNameWithClient",
                Font = new Font("Segoe UI", 9)
            };
            lstProjects.DoubleClick += LstProjects_DoubleClick;
            lstProjects.SelectedIndexChanged += LstProjects_SelectedIndexChanged;
            this.Controls.Add(lstProjects);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(12, 385),
                Size = new Size(558, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            this.Controls.Add(progressBar);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(12, 410),
                Size = new Size(558, 20),
                AutoSize = false,
                ForeColor = Color.Blue,
                Text = "Loading projects..."
            };
            this.Controls.Add(lblStatus);

            // Select button
            btnSelect = new Button
            {
                Text = "Select Project",
                Location = new Point(495, 440),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            btnSelect.Click += BtnSelect_Click;
            this.Controls.Add(btnSelect);

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(395, 440),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSelect;
            this.CancelButton = btnCancel;
        }

        private async void LoadProjectsAsync()
        {
            try
            {
                SetLoadingState(true, "Loading projects from Timely...");

                if (_projectService == null)
                {
                    throw new InvalidOperationException("Project service not initialized.");
                }

                _allProjects = await _projectService.FetchProjectsAsync();
                _filteredProjects = new List<TimelyProject>(_allProjects);

                UpdateProjectsList();
                
                if (_allProjects.Count == 0)
                {
                    SetStatusMessage("No active projects found in your Timely workspace.", Color.Orange);
                }
                else
                {
                    SetStatusMessage($"Loaded {_allProjects.Count} projects successfully.", Color.Green);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                SetStatusMessage("Authentication failed. Please update your Timely configuration.", Color.Red);
                ShowErrorMessage("Authentication Error", ex.Message + "\n\nPlease configure Timely with fresh authentication cookies.");
            }
            catch (InvalidOperationException ex)
            {
                SetStatusMessage("Configuration error. Please check your Timely settings.", Color.Red);
                ShowErrorMessage("Configuration Error", ex.Message);
            }
            catch (Exception ex)
            {
                SetStatusMessage("Failed to load projects. Check your connection and configuration.", Color.Red);
                ShowErrorMessage("Error Loading Projects", $"An error occurred while loading projects:\n\n{ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading, string? message = null)
        {
            if (progressBar != null) progressBar.Visible = isLoading;
            if (btnRefresh != null) btnRefresh.Enabled = !isLoading;
            if (txtSearch != null) txtSearch.Enabled = !isLoading;
            if (lstProjects != null) lstProjects.Enabled = !isLoading;
            
            if (message != null && lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = Color.Blue;
            }
        }

        private void SetStatusMessage(string message, Color color)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        private void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateProjectsList()
        {
            if (lstProjects == null) return;

            lstProjects.BeginUpdate();
            lstProjects.Items.Clear();
            
            foreach (var project in _filteredProjects)
            {
                lstProjects.Items.Add(project);
            }
            
            lstProjects.EndUpdate();
            
            // Update button state
            if (btnSelect != null)
            {
                btnSelect.Enabled = lstProjects.SelectedIndex >= 0;
            }
        }

        private void FilterProjects()
        {
            if (txtSearch == null) return;

            var searchTerm = txtSearch.Text.Trim();
            _filteredProjects = TimelyProjectService.SearchProjects(_allProjects, searchTerm);
            
            UpdateProjectsList();
            
            // Update status
            var totalCount = _allProjects.Count;
            var filteredCount = _filteredProjects.Count;
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                SetStatusMessage($"Showing all {totalCount} projects.", Color.Blue);
            }
            else
            {
                SetStatusMessage($"Found {filteredCount} of {totalCount} projects matching '{searchTerm}'.", Color.Blue);
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            FilterProjects();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadProjectsAsync();
        }

        private void LstProjects_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (btnSelect != null && lstProjects != null)
            {
                btnSelect.Enabled = lstProjects.SelectedIndex >= 0;
            }
        }

        private void LstProjects_DoubleClick(object? sender, EventArgs e)
        {
            if (lstProjects?.SelectedItem is TimelyProject)
            {
                BtnSelect_Click(sender, e);
            }
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            if (lstProjects?.SelectedItem is TimelyProject selectedProject)
            {
                SelectedProject = selectedProject;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a project from the list.", "No Project Selected",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _projectService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
