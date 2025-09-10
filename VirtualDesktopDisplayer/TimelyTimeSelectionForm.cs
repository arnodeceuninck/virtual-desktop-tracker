using System;
using System.Drawing;
using System.Windows.Forms;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Dialog form for selecting a cutoff time for Timely upload.
    /// Only records with an end time after the selected time will be uploaded.
    /// </summary>
    public partial class TimelyTimeSelectionForm : Form
    {
        private DateTimePicker? _dateTimePicker;
        private Label? _instructionLabel;
        private Label? _descriptionLabel;
        private Button? _okButton;
        private Button? _cancelButton;

        /// <summary>
        /// Gets the selected cutoff time.
        /// </summary>
        public DateTime SelectedTime => _dateTimePicker?.Value ?? DateTime.Now;

        public TimelyTimeSelectionForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Upload Time Range";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Instruction label
            _instructionLabel = new Label()
            {
                Text = "Upload to Timely from specific time",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(400, 25),
                Location = new Point(20, 20),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(_instructionLabel);

            // Description label
            _descriptionLabel = new Label()
            {
                Text = "Only today's records with an end time after the selected moment will be uploaded.\n" +
                      "This allows you to upload only recent activity from today or continue from where you left off.",
                Size = new Size(390, 50),
                Location = new Point(20, 50),
                ForeColor = Color.DarkSlateGray
            };
            this.Controls.Add(_descriptionLabel);

            // Date/Time picker
            var timeLabel = new Label()
            {
                Text = "Upload records ending after:",
                Size = new Size(200, 20),
                Location = new Point(20, 120),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            this.Controls.Add(timeLabel);

            _dateTimePicker = new DateTimePicker()
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Size = new Size(160, 25),
                Location = new Point(20, 145),
                Value = DateTime.Now.AddHours(-2) // Default to 2 hours ago
            };
            this.Controls.Add(_dateTimePicker);

            // Example label
            var exampleLabel = new Label()
            {
                Text = "Tip: Set to 2-3 hours ago to upload recent work only.",
                Size = new Size(300, 20),
                Location = new Point(20, 175),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };
            this.Controls.Add(exampleLabel);

            // OK button
            _okButton = new Button()
            {
                Text = "Upload",
                Size = new Size(80, 30),
                Location = new Point(250, 210),
                DialogResult = DialogResult.OK,
                UseVisualStyleBackColor = true
            };
            _okButton.Click += OnOkClick;
            this.Controls.Add(_okButton);

            // Cancel button
            _cancelButton = new Button()
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                Location = new Point(340, 210),
                DialogResult = DialogResult.Cancel,
                UseVisualStyleBackColor = true
            };
            this.Controls.Add(_cancelButton);

            // Set default button
            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void OnOkClick(object? sender, EventArgs e)
        {
            if (_dateTimePicker == null) return;

            // Validate that the selected time is not in the future
            if (_dateTimePicker.Value > DateTime.Now)
            {
                MessageBox.Show(
                    "The selected time cannot be in the future. Please select a past time.",
                    "Invalid Time Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Validate that the selected time is not too far in the past (more than 30 days)
            if (_dateTimePicker.Value < DateTime.Now.AddDays(-30))
            {
                var result = MessageBox.Show(
                    "The selected time is more than 30 days ago. This might result in uploading a large amount of data.\n\n" +
                    "Are you sure you want to continue?",
                    "Large Time Range",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
