using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Form for selecting labels for a Timely project.
    /// </summary>
    public partial class TimelyLabelSelectorForm : Form
    {
        private readonly TimelyProjectDetails _projectDetails;
        private TimelyLabelService? _labelService;
        private List<TimelyLabel> _allLabels = new List<TimelyLabel>();
        private List<TimelyLabel> _projectLabels = new List<TimelyLabel>();
        private List<TimelyLabel> _filteredLabels = new List<TimelyLabel>();

        // UI Components
        private Label? lblProjectInfo;
        private Label? lblRequiredWarning;
        private TextBox? txtSearch;
        private CheckedListBox? clbLabels;
        private Button? btnRefresh;
        private Button? btnSelectAll;
        private Button? btnClearAll;
        private Button? btnOK;
        private Button? btnCancel;
        private Label? lblStatus;
        private ProgressBar? progressBar;

        /// <summary>
        /// The selected label IDs.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<long> SelectedLabelIds { get; set; } = new List<long>();

        public TimelyLabelSelectorForm(TimelyProjectDetails projectDetails)
        {
            _projectDetails = projectDetails ?? throw new ArgumentNullException(nameof(projectDetails));
            _labelService = new TimelyLabelService();
            InitializeComponent();
            LoadLabelsAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Labels";
            this.Size = new Size(720, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Project info
            lblProjectInfo = new Label
            {
                Text = $"Project: {_projectDetails.Name}",
                Location = new Point(12, 12),
                Size = new Size(680, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = false
            };
            this.Controls.Add(lblProjectInfo);

            // Required labels warning
            if (_projectDetails.RequiredLabels)
            {
                lblRequiredWarning = new Label
                {
                    Text = "⚠️ This project requires labels to be selected for time tracking!",
                    Location = new Point(12, 40),
                    Size = new Size(680, 20),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.Red,
                    AutoSize = false
                };
                this.Controls.Add(lblRequiredWarning);
            }

            // Search box
            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(12, _projectDetails.RequiredLabels ? 70 : 50),
                Size = new Size(50, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(70, _projectDetails.RequiredLabels ? 70 : 50),
                Size = new Size(520, 23),
                PlaceholderText = "Type to search labels by name..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.Controls.Add(txtSearch);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(600, _projectDetails.RequiredLabels ? 70 : 50),
                Size = new Size(70, 25)
            };
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            // Action buttons
            var buttonY = _projectDetails.RequiredLabels ? 105 : 85;
            
            btnSelectAll = new Button
            {
                Text = "Select All",
                Location = new Point(12, buttonY),
                Size = new Size(80, 25)
            };
            btnSelectAll.Click += BtnSelectAll_Click;
            this.Controls.Add(btnSelectAll);

            btnClearAll = new Button
            {
                Text = "Clear All",
                Location = new Point(100, buttonY),
                Size = new Size(80, 25)
            };
            btnClearAll.Click += BtnClearAll_Click;
            this.Controls.Add(btnClearAll);

            // Labels list
            clbLabels = new CheckedListBox
            {
                Location = new Point(12, buttonY + 35),
                Size = new Size(680, 350),
                CheckOnClick = true,
                Font = new Font("Segoe UI", 9),
                ScrollAlwaysVisible = true
            };
            clbLabels.ItemCheck += ClbLabels_ItemCheck;
            this.Controls.Add(clbLabels);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(12, buttonY + 395),
                Size = new Size(680, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            this.Controls.Add(progressBar);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(12, buttonY + 420),
                Size = new Size(680, 20),
                AutoSize = false,
                ForeColor = Color.Blue,
                Text = "Loading labels..."
            };
            this.Controls.Add(lblStatus);

            // OK button
            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(615, buttonY + 450),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(525, buttonY + 450),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private async void LoadLabelsAsync()
        {
            try
            {
                SetLoadingState(true, "Loading labels from Timely...");

                if (_labelService == null)
                {
                    throw new InvalidOperationException("Label service not initialized.");
                }

                // Load all labels first
                _allLabels = await _labelService.FetchLabelsAsync();
                
                // Get the labels available for this project
                _projectLabels = _labelService.GetProjectLabels(_projectDetails, _allLabels);
                _filteredLabels = new List<TimelyLabel>(_projectLabels);

                UpdateLabelsList();
                
                if (_projectLabels.Count == 0)
                {
                    SetStatusMessage("No labels are configured for this project.", Color.Orange);
                }
                else
                {
                    var requiredCount = _projectDetails.RequiredLabelIds?.Count ?? 0;
                    var defaultCount = _projectDetails.DefaultLabelIds?.Count ?? 0;
                    
                    var statusMessage = $"Loaded {_projectLabels.Count} available labels for this project.";
                    if (requiredCount > 0)
                    {
                        statusMessage += $" ({requiredCount} required)";
                    }
                    if (defaultCount > 0)
                    {
                        statusMessage += $" ({defaultCount} default)";
                    }
                    
                    SetStatusMessage(statusMessage, Color.Green);
                }

                // Pre-select default labels and any previously selected labels
                if (_projectDetails.DefaultLabelIds != null && _projectDetails.DefaultLabelIds.Count > 0)
                {
                    SelectDefaultLabels();
                }
                
                // Also pre-select any labels that were previously selected
                SelectPreviouslySelectedLabels();
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
                SetStatusMessage("Failed to load labels. Check your connection and configuration.", Color.Red);
                ShowErrorMessage("Error Loading Labels", $"An error occurred while loading labels:\n\n{ex.Message}");
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
            if (clbLabels != null) clbLabels.Enabled = !isLoading;
            if (btnSelectAll != null) btnSelectAll.Enabled = !isLoading;
            if (btnClearAll != null) btnClearAll.Enabled = !isLoading;
            
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

        private void UpdateLabelsList()
        {
            if (clbLabels == null) return;

            clbLabels.BeginUpdate();
            
            // Remember current selections
            var selectedIds = new HashSet<long>();
            for (int i = 0; i < clbLabels.Items.Count; i++)
            {
                if (clbLabels.GetItemChecked(i) && clbLabels.Items[i] is TimelyLabel label)
                {
                    selectedIds.Add(label.Id);
                }
            }

            clbLabels.Items.Clear();
            
            // Group labels by parent for better organization
            var rootLabels = _filteredLabels.Where(l => !l.IsChild).OrderBy(l => l.Sequence).ThenBy(l => l.Name).ToList();
            var childLabels = _filteredLabels.Where(l => l.IsChild).ToList();

            foreach (var rootLabel in rootLabels)
            {
                // Add root label
                var rootIndex = clbLabels.Items.Add(rootLabel);
                if (selectedIds.Contains(rootLabel.Id))
                {
                    clbLabels.SetItemChecked(rootIndex, true);
                }

                // Add child labels under this root
                var children = childLabels.Where(c => c.ParentId == rootLabel.Id)
                                         .OrderBy(c => c.Sequence)
                                         .ThenBy(c => c.Name)
                                         .ToList();

                foreach (var child in children)
                {
                    // Indent child labels for visual hierarchy
                    var displayLabel = new TimelyLabel
                    {
                        Id = child.Id,
                        Name = $"    {child.Name}",
                        FullPath = child.FullPath,
                        Active = child.Active,
                        Sequence = child.Sequence,
                        ParentId = child.ParentId
                    };

                    var childIndex = clbLabels.Items.Add(displayLabel);
                    if (selectedIds.Contains(child.Id))
                    {
                        clbLabels.SetItemChecked(childIndex, true);
                    }
                }
            }

            // Add orphan child labels (children without visible parents)
            var visibleParentIds = rootLabels.Select(l => l.Id).ToHashSet();
            var orphanChildren = childLabels.Where(c => c.ParentId.HasValue && !visibleParentIds.Contains(c.ParentId.Value))
                                           .OrderBy(c => c.Sequence)
                                           .ThenBy(c => c.Name)
                                           .ToList();

            foreach (var orphan in orphanChildren)
            {
                var orphanIndex = clbLabels.Items.Add(orphan);
                if (selectedIds.Contains(orphan.Id))
                {
                    clbLabels.SetItemChecked(orphanIndex, true);
                }
            }

            clbLabels.EndUpdate();
            
            UpdateSelectedCount();
        }

        private void SelectDefaultLabels()
        {
            if (clbLabels == null || _projectDetails.DefaultLabelIds == null) return;

            var defaultIds = new HashSet<long>(_projectDetails.DefaultLabelIds);

            for (int i = 0; i < clbLabels.Items.Count; i++)
            {
                if (clbLabels.Items[i] is TimelyLabel label && defaultIds.Contains(label.Id))
                {
                    clbLabels.SetItemChecked(i, true);
                }
            }
        }

        private void SelectPreviouslySelectedLabels()
        {
            if (clbLabels == null || SelectedLabelIds == null || SelectedLabelIds.Count == 0) return;

            var selectedIds = new HashSet<long>(SelectedLabelIds);

            for (int i = 0; i < clbLabels.Items.Count; i++)
            {
                if (clbLabels.Items[i] is TimelyLabel label && selectedIds.Contains(label.Id))
                {
                    clbLabels.SetItemChecked(i, true);
                }
            }
        }

        private void UpdateSelectedCount()
        {
            if (clbLabels == null) return;

            var selectedCount = clbLabels.CheckedItems.Count;
            var totalCount = _filteredLabels.Count;

            var countText = $" ({selectedCount} of {totalCount} selected)";
            
            if (lblStatus != null && !lblStatus.Text.Contains("(") && !lblStatus.Text.Contains("Loading") && !lblStatus.Text.Contains("Failed"))
            {
                var baseMessage = lblStatus.Text;
                if (baseMessage.Contains("("))
                {
                    baseMessage = baseMessage.Substring(0, baseMessage.IndexOf("(")).Trim();
                }
                lblStatus.Text = baseMessage + countText;
            }
        }

        private void FilterLabels()
        {
            if (txtSearch == null) return;

            var searchTerm = txtSearch.Text.Trim();
            _filteredLabels = TimelyLabelService.SearchLabels(_projectLabels, searchTerm);
            
            UpdateLabelsList();
            
            var filteredCount = _filteredLabels.Count;
            var totalCount = _projectLabels.Count;
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                SetStatusMessage($"Showing all {totalCount} available labels for this project.", Color.Blue);
            }
            else
            {
                SetStatusMessage($"Found {filteredCount} labels matching '{searchTerm}'.", Color.Blue);
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            FilterLabels();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadLabelsAsync();
        }

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            if (clbLabels == null) return;

            for (int i = 0; i < clbLabels.Items.Count; i++)
            {
                clbLabels.SetItemChecked(i, true);
            }
        }

        private void BtnClearAll_Click(object? sender, EventArgs e)
        {
            if (clbLabels == null) return;

            for (int i = 0; i < clbLabels.Items.Count; i++)
            {
                clbLabels.SetItemChecked(i, false);
            }
        }

        private void ClbLabels_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Use BeginInvoke to update count after the check state has changed
            this.BeginInvoke(new Action(UpdateSelectedCount));
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (clbLabels == null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            // Collect selected label IDs
            SelectedLabelIds.Clear();
            foreach (TimelyLabel label in clbLabels.CheckedItems)
            {
                SelectedLabelIds.Add(label.Id);
            }

            // Validate required labels if project has label requirements
            if (_projectDetails.RequiredLabels && SelectedLabelIds.Count == 0)
            {
                MessageBox.Show("This project requires at least one label to be selected for time tracking.", 
                              "Labels Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _labelService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
