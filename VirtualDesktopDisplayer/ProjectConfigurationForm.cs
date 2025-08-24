using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;

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

        public ProjectConfigurationForm()
        {
            _projectConfig = ProjectConfiguration.Instance;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Project Configuration";
            this.Size = new Size(800, 600);
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
                Height = 120
            };

            var defaultPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };

            defaultPanel.Controls.Add(new Label { Text = "Project ID:", Anchor = AnchorStyles.Left }, 0, 0);
            _defaultIdTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            defaultPanel.Controls.Add(_defaultIdTextBox, 1, 0);

            defaultPanel.Controls.Add(new Label { Text = "Project Name:", Anchor = AnchorStyles.Left }, 0, 1);
            _defaultNameTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            defaultPanel.Controls.Add(_defaultNameTextBox, 1, 1);

            defaultPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            defaultPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

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
                Width = 300,
                DataPropertyName = "KeywordsString"
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

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            this.Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            // Load default project
            _defaultIdTextBox.Text = _projectConfig.DefaultProject.Id.ToString();
            _defaultNameTextBox.Text = _projectConfig.DefaultProject.Name;

            // Load project mappings
            var dataSource = _projectConfig.ProjectMappings.Select(m => new ProjectMappingViewModel
            {
                ProjectId = m.Project.Id,
                ProjectName = m.Project.Name,
                KeywordsString = string.Join(", ", m.Keywords)
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
                    KeywordsString = string.Join(", ", form.Keywords)
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

                    _projectConfig.ProjectMappings.Add(new ProjectMapping
                    {
                        Project = new ProjectInfo { Id = item.ProjectId, Name = item.ProjectName },
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
    }

    /// <summary>
    /// View model for displaying project mappings in the grid.
    /// </summary>
    public class ProjectMappingViewModel
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public string KeywordsString { get; set; } = "";
    }

    /// <summary>
    /// Simple form for editing a single project mapping.
    /// </summary>
    public partial class ProjectMappingEditForm : Form
    {
        public long ProjectId { get; private set; }
        public string ProjectName { get; private set; } = "";
        public List<string> Keywords { get; private set; } = new List<string>();

        private TextBox _idTextBox;
        private TextBox _nameTextBox;
        private TextBox _keywordsTextBox;

        public ProjectMappingEditForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Project Mapping";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            layout.Controls.Add(new Label { Text = "Project ID:", Anchor = AnchorStyles.Left }, 0, 0);
            _idTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_idTextBox, 1, 0);

            layout.Controls.Add(new Label { Text = "Project Name:", Anchor = AnchorStyles.Left }, 0, 1);
            _nameTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_nameTextBox, 1, 1);

            layout.Controls.Add(new Label { Text = "Keywords:", Anchor = AnchorStyles.Left }, 0, 2);
            _keywordsTextBox = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            layout.Controls.Add(_keywordsTextBox, 1, 2);

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
                }
            };

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);

            layout.Controls.Add(buttonPanel, 1, 3);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            this.Controls.Add(layout);
        }
    }
}
