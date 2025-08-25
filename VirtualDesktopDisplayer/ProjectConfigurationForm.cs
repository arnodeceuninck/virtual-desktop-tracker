using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Form for configuring project keyword mappings.
    /// </summary>
    public partial class ProjectConfigurationForm : Form
    {
        private readonly ProjectConfiguration _projectConfig;
        private DataGridView _projectGrid;
        private Button _addButton;
        private Button _removeButton;
        private Button _saveButton;
        private Button _cancelButton;
        private GroupBox _defaultProjectGroup;
        private TextBox _defaultIdTextBox;
        private TextBox _defaultNameTextBox;
        private TextBox _defaultLabelIdsTextBox;
        private Button _selectDefaultProjectButton;
        private Button _selectDefaultLabelsButton;

        public ProjectConfigurationForm()
        {
            _projectConfig = ProjectConfiguration.Instance;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Project Configuration";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            // Default project section
            _defaultProjectGroup = new GroupBox
            {
                Text = "Default Project",
                Height = 180,
                Dock = DockStyle.Fill
            };

            var defaultPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(10)
            };

            defaultPanel.Controls.Add(new Label { Text = "Project ID:", Anchor = AnchorStyles.Left }, 0, 0);
            _defaultIdTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            defaultPanel.Controls.Add(_defaultIdTextBox, 1, 0);
            
            _selectDefaultProjectButton = new Button 
            { 
                Text = "Select from Timely",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Height = 30
            };
            _selectDefaultProjectButton.Click += SelectDefaultProjectButton_Click;
            defaultPanel.Controls.Add(_selectDefaultProjectButton, 2, 0);

            defaultPanel.Controls.Add(new Label { Text = "Project Name:", Anchor = AnchorStyles.Left }, 0, 1);
            _defaultNameTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            defaultPanel.Controls.Add(_defaultNameTextBox, 1, 1);
            defaultPanel.Controls.Add(new Label(), 2, 1); // Empty cell

            defaultPanel.Controls.Add(new Label { Text = "Label IDs:", Anchor = AnchorStyles.Left }, 0, 2);
            _defaultLabelIdsTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            defaultPanel.Controls.Add(_defaultLabelIdsTextBox, 1, 2);
            
            _selectDefaultLabelsButton = new Button 
            { 
                Text = "Select Labels",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Height = 30,
                Enabled = false
            };
            _selectDefaultLabelsButton.Click += SelectDefaultLabelsButton_Click;
            defaultPanel.Controls.Add(_selectDefaultLabelsButton, 2, 2);

            defaultPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            defaultPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            defaultPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            _defaultProjectGroup.Controls.Add(defaultPanel);

            // Project mappings grid
            var gridPanel = new Panel { Dock = DockStyle.Fill };
            var gridLabel = new Label
            {
                Text = "Project Keyword Mappings:",
                Dock = DockStyle.Top,
                Height = 25
            };

            _projectGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            // Add columns
            _projectGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProjectId",
                HeaderText = "Project ID",
                Width = 100,
                DataPropertyName = "ProjectId"
            });

            _projectGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProjectName",
                HeaderText = "Project Name",
                Width = 250,
                DataPropertyName = "ProjectName"
            });

            _projectGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Keywords",
                HeaderText = "Keywords (comma separated)",
                Width = 250,
                DataPropertyName = "KeywordsString"
            });

            _projectGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LabelIds",
                HeaderText = "Label IDs (comma separated)",
                Width = 200,
                DataPropertyName = "LabelIdsString"
            });

            gridPanel.Controls.Add(_projectGrid);
            gridPanel.Controls.Add(gridLabel);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(5)
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.Click += (s, e) => this.Close();

            _saveButton = new Button
            {
                Text = "Save",
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _saveButton.Click += SaveButton_Click;

            _removeButton = new Button
            {
                Text = "Remove",
                Size = new Size(75, 30)
            };
            _removeButton.Click += RemoveButton_Click;

            _addButton = new Button
            {
                Text = "Add",
                Size = new Size(75, 30)
            };
            _addButton.Click += AddButton_Click;

            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_saveButton);
            buttonPanel.Controls.Add(_removeButton);
            buttonPanel.Controls.Add(_addButton);

            // Add to main panel
            mainPanel.Controls.Add(_defaultProjectGroup, 0, 0);
            mainPanel.Controls.Add(gridPanel, 0, 1);
            mainPanel.Controls.Add(buttonPanel, 0, 2);

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            this.Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            // Load default project
            _defaultIdTextBox.Text = _projectConfig.DefaultProject.Id.ToString();
            _defaultNameTextBox.Text = _projectConfig.DefaultProject.Name;
            _defaultLabelIdsTextBox.Text = string.Join(", ", _projectConfig.DefaultProject.LabelIds);

            // Enable label selection if we have a project
            _selectDefaultLabelsButton.Enabled = _projectConfig.DefaultProject.Id > 0;

            // Load project mappings
            var dataSource = _projectConfig.ProjectMappings.Select(m => new ProjectMappingViewModel
            {
                ProjectId = m.Project.Id,
                ProjectName = m.Project.Name,
                KeywordsString = string.Join(", ", m.Keywords),
                LabelIdsString = string.Join(", ", m.Project.LabelIds)
            }).ToList();

            _projectGrid.DataSource = dataSource;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var form = new ProjectMappingEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var dataSource = (List<ProjectMappingViewModel>)_projectGrid.DataSource;
                dataSource.Add(new ProjectMappingViewModel
                {
                    ProjectId = form.ProjectId,
                    ProjectName = form.ProjectName,
                    KeywordsString = string.Join(", ", form.Keywords),
                    LabelIdsString = string.Join(", ", form.LabelIds)
                });

                _projectGrid.DataSource = null;
                _projectGrid.DataSource = dataSource;
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_projectGrid.SelectedRows.Count > 0)
            {
                var dataSource = (List<ProjectMappingViewModel>)_projectGrid.DataSource;
                var selectedIndex = _projectGrid.SelectedRows[0].Index;
                dataSource.RemoveAt(selectedIndex);

                _projectGrid.DataSource = null;
                _projectGrid.DataSource = dataSource;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Update default project
                if (long.TryParse(_defaultIdTextBox.Text, out long defaultId))
                {
                    _projectConfig.DefaultProject.Id = defaultId;
                    _projectConfig.DefaultProject.Name = _defaultNameTextBox.Text;
                    
                    var defaultLabelIds = _defaultLabelIdsTextBox.Text
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out _))
                        .Select(l => int.Parse(l))
                        .ToList();
                    _projectConfig.DefaultProject.LabelIds = defaultLabelIds;
                }

                // Update project mappings
                var dataSource = (List<ProjectMappingViewModel>)_projectGrid.DataSource;
                _projectConfig.ProjectMappings.Clear();

                foreach (var item in dataSource)
                {
                    var keywords = item.KeywordsString
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k))
                        .ToList();

                    var labelIds = item.LabelIdsString
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out _))
                        .Select(l => int.Parse(l))
                        .ToList();

                    _projectConfig.ProjectMappings.Add(new ProjectMapping
                    {
                        Project = new ProjectInfo 
                        { 
                            Id = item.ProjectId, 
                            Name = item.ProjectName,
                            LabelIds = labelIds
                        },
                        Keywords = keywords
                    });
                }

                _projectConfig.SaveConfiguration();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectDefaultProjectButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var timelyConfig = TimelyConfiguration.Instance;
                if (string.IsNullOrEmpty(timelyConfig.CookieString) || string.IsNullOrEmpty(timelyConfig.WorkspaceId))
                {
                    MessageBox.Show("Timely configuration is required. Please configure Timely first.",
                        "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var projectSelector = new TimelyProjectSelectorForm();
                if (projectSelector.ShowDialog() == DialogResult.OK && projectSelector.SelectedProject != null)
                {
                    var selectedProject = projectSelector.SelectedProject;
                    _defaultIdTextBox.Text = selectedProject.Id.ToString();
                    _defaultNameTextBox.Text = selectedProject.Name;
                    // Note: Label IDs would need to be set separately as they're not in the basic project info
                    
                    // Enable the labels button now that we have a project
                    _selectDefaultLabelsButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving projects: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SelectDefaultLabelsButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Get the current project ID
                if (!long.TryParse(_defaultIdTextBox.Text, out long projectId) || projectId <= 0)
                {
                    MessageBox.Show(
                        "Please select a project first before configuring labels.",
                        "No Project Selected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var timelyConfig = TimelyConfiguration.Instance;
                if (string.IsNullOrEmpty(timelyConfig.CookieString) || string.IsNullOrEmpty(timelyConfig.WorkspaceId))
                {
                    MessageBox.Show(
                        "Timely configuration is required. Please configure Timely first.",
                        "Configuration Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (var labelService = new VirtualDesktopHelper.Services.TimelyLabelService())
                {
                    // Fetch detailed project information
                    var projectDetails = await labelService.FetchProjectDetailsAsync(projectId);

                    // Show label selector dialog
                    using (var labelForm = new TimelyLabelSelectorForm(projectDetails))
                    {
                        // Pre-select any previously selected labels
                        var currentLabelIds = ParseLabelIds(_defaultLabelIdsTextBox.Text);
                        labelForm.SelectedLabelIds = new List<long>(currentLabelIds);

                        if (labelForm.ShowDialog(this) == DialogResult.OK)
                        {
                            // Update the label IDs text box
                            _defaultLabelIdsTextBox.Text = string.Join(", ", labelForm.SelectedLabelIds);

                            var labelCount = labelForm.SelectedLabelIds.Count;
                            var labelText = labelCount == 1 ? "label" : "labels";
                            MessageBox.Show(
                                $"Selected {labelCount} {labelText} for the default project.",
                                "Labels Updated",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Authentication failed. Please update your Timely configuration with fresh authentication cookies.",
                    "Authentication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading project labels: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private List<long> ParseLabelIds(string labelIdsText)
        {
            var labelIds = new List<long>();
            if (!string.IsNullOrWhiteSpace(labelIdsText))
            {
                var parts = labelIdsText.Split(',');
                foreach (var part in parts)
                {
                    if (long.TryParse(part.Trim(), out long id))
                    {
                        labelIds.Add(id);
                    }
                }
            }
            return labelIds;
        }
    }

    /// <summary>
    /// View model for displaying project mappings in the grid.
    /// </summary>
    public class ProjectMappingViewModel
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public string KeywordsString { get; set; } = "";
        public string LabelIdsString { get; set; } = "";
    }

    /// <summary>
    /// Simple form for editing a single project mapping.
    /// </summary>
    public partial class ProjectMappingEditForm : Form
    {
        public long ProjectId { get; private set; }
        public string ProjectName { get; private set; } = "";
        public List<string> Keywords { get; private set; } = new List<string>();
        public List<int> LabelIds { get; private set; } = new List<int>();

        private TextBox _idTextBox;
        private TextBox _nameTextBox;
        private TextBox _keywordsTextBox;
        private TextBox _labelIdsTextBox;
        private Button _selectProjectButton;
        private Button _selectLabelsButton;

        public ProjectMappingEditForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Project Mapping";
            this.Size = new Size(500, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 5,
                Padding = new Padding(10)
            };

            // Project ID row with select button
            layout.Controls.Add(new Label { Text = "Project ID:", Anchor = AnchorStyles.Left }, 0, 0);
            _idTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_idTextBox, 1, 0);
            
            _selectProjectButton = new Button 
            { 
                Text = "Select from Timely",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Height = 30
            };
            _selectProjectButton.Click += SelectProjectButton_Click;
            layout.Controls.Add(_selectProjectButton, 2, 0);

            layout.Controls.Add(new Label { Text = "Project Name:", Anchor = AnchorStyles.Left }, 0, 1);
            _nameTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_nameTextBox, 1, 1);
            layout.Controls.Add(new Label(), 2, 1); // Empty cell

            layout.Controls.Add(new Label { Text = "Keywords:", Anchor = AnchorStyles.Left }, 0, 2);
            _keywordsTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_keywordsTextBox, 1, 2);
            layout.Controls.Add(new Label(), 2, 2); // Empty cell

            layout.Controls.Add(new Label { Text = "Label IDs:", Anchor = AnchorStyles.Left }, 0, 3);
            _labelIdsTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_labelIdsTextBox, 1, 3);
            
            _selectLabelsButton = new Button 
            { 
                Text = "Select Labels",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Height = 30,
                Enabled = false
            };
            _selectLabelsButton.Click += SelectLabelsButton_Click;
            layout.Controls.Add(_selectLabelsButton, 2, 3);

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK
            };
            okButton.Click += (s, e) =>
            {
                if (long.TryParse(_idTextBox.Text, out long id))
                {
                    ProjectId = id;
                    ProjectName = _nameTextBox.Text;
                    Keywords = _keywordsTextBox.Text
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrEmpty(k))
                        .ToList();
                    LabelIds = _labelIdsTextBox.Text
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrEmpty(l) && int.TryParse(l, out _))
                        .Select(l => int.Parse(l))
                        .ToList();
                }
            };

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);

            layout.Controls.Add(buttonPanel, 1, 4);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            this.Controls.Add(layout);
        }

        private void SelectProjectButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var timelyConfig = TimelyConfiguration.Instance;
                if (string.IsNullOrEmpty(timelyConfig.CookieString) || string.IsNullOrEmpty(timelyConfig.WorkspaceId))
                {
                    MessageBox.Show("Timely configuration is required. Please configure Timely first.",
                        "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var projectSelector = new TimelyProjectSelectorForm();
                if (projectSelector.ShowDialog() == DialogResult.OK && projectSelector.SelectedProject != null)
                {
                    var selectedProject = projectSelector.SelectedProject;
                    _idTextBox.Text = selectedProject.Id.ToString();
                    _nameTextBox.Text = selectedProject.Name;
                    // Note: Label IDs would need to be set separately as they're not in the basic project info
                    
                    // Enable the labels button now that we have a project
                    _selectLabelsButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving projects: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SelectLabelsButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Get the current project ID
                if (!long.TryParse(_idTextBox.Text, out long projectId) || projectId <= 0)
                {
                    MessageBox.Show(
                        "Please select a project first before configuring labels.",
                        "No Project Selected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var timelyConfig = TimelyConfiguration.Instance;
                if (string.IsNullOrEmpty(timelyConfig.CookieString) || string.IsNullOrEmpty(timelyConfig.WorkspaceId))
                {
                    MessageBox.Show(
                        "Timely configuration is required. Please configure Timely first.",
                        "Configuration Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (var labelService = new VirtualDesktopHelper.Services.TimelyLabelService())
                {
                    // Fetch detailed project information
                    var projectDetails = await labelService.FetchProjectDetailsAsync(projectId);

                    // Show label selector dialog
                    using (var labelForm = new TimelyLabelSelectorForm(projectDetails))
                    {
                        // Pre-select any previously selected labels
                        var currentLabelIds = ParseCurrentLabelIds();
                        labelForm.SelectedLabelIds = new List<long>(currentLabelIds);

                        if (labelForm.ShowDialog(this) == DialogResult.OK)
                        {
                            // Update the label IDs text box
                            _labelIdsTextBox.Text = string.Join(", ", labelForm.SelectedLabelIds);

                            var labelCount = labelForm.SelectedLabelIds.Count;
                            var labelText = labelCount == 1 ? "label" : "labels";
                            MessageBox.Show(
                                $"Selected {labelCount} {labelText} for this project mapping.",
                                "Labels Updated",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Authentication failed. Please update your Timely configuration with fresh authentication cookies.",
                    "Authentication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading project labels: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private List<long> ParseCurrentLabelIds()
        {
            var labelIds = new List<long>();
            if (!string.IsNullOrWhiteSpace(_labelIdsTextBox.Text))
            {
                var parts = _labelIdsTextBox.Text.Split(',');
                foreach (var part in parts)
                {
                    if (long.TryParse(part.Trim(), out long id))
                    {
                        labelIds.Add(id);
                    }
                }
            }
            return labelIds;
        }
    }
}
