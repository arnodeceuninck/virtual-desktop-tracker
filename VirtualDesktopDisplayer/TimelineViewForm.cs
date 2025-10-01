using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopDisplayer
{
    /// <summary>
    /// Form for displaying detailed and consolidated timings side by side in a timeline view.
    /// </summary>
    public partial class TimelineViewForm : Form
    {
        private readonly IDesktopUsageTracker _usageTracker;
        private readonly IUsageConsolidationService _consolidationService;
        private readonly ProjectDetectionService _projectDetectionService;
        private readonly TimelyProjectService _timelyProjectService;

        private Panel _detailedTimelinePanel;
        private Panel _consolidatedTimelinePanel;
        private Panel _legendPanel;
        private DateTimePicker _datePicker;
        private Button _refreshButton;
        private Button _previousDayButton;
        private Button _nextDayButton;
        private Button _openInTimelyButton;
        private Label _statusLabel;

        private List<DesktopUsageEntry> _detailedEntries = new List<DesktopUsageEntry>();
        private List<DesktopUsageEntry> _consolidatedEntries = new List<DesktopUsageEntry>();
        private Dictionary<long, Color> _projectColors = new Dictionary<long, Color>();
        private Dictionary<DesktopUsageEntry, ProjectInfo> _entryProjectMappings = new Dictionary<DesktopUsageEntry, ProjectInfo>();

        private DateTime _timeRangeStart;
        private DateTime _timeRangeEnd;
        private double _minutesPerPixel;

        // Timeline display constants
        private const int TIMELINE_HEIGHT = 600;
        private const int TIMELINE_WIDTH = 400;
        private const int HOUR_HEIGHT = 30;
        private const int MARGIN = 10;
        private const int TIME_SCALE_WIDTH = 60;

        public TimelineViewForm(IDesktopUsageTracker? usageTracker = null)
        {
            _usageTracker = usageTracker ?? new DesktopUsageTracker();
            _consolidationService = new UsageConsolidationService();
            _projectDetectionService = new ProjectDetectionService();
            _timelyProjectService = new TimelyProjectService();

            InitializeComponent();
            LoadProjectColors();
            LoadDataForToday();
        }

        private void InitializeComponent()
        {
            this.Text = "Timeline View - Detailed vs Consolidated";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Date picker and controls
            _datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                Size = new Size(150, 25)
            };

            _refreshButton = new Button
            {
                Text = "Refresh",
                Size = new Size(80, 25),
                UseVisualStyleBackColor = true
            };

            _previousDayButton = new Button
            {
                Text = "◀ Previous",
                Size = new Size(80, 25),
                UseVisualStyleBackColor = true
            };

            _nextDayButton = new Button
            {
                Text = "Next ▶",
                Size = new Size(80, 25),
                UseVisualStyleBackColor = true
            };

            _openInTimelyButton = new Button
            {
                Text = "Upload to Timely",
                Size = new Size(120, 25),
                UseVisualStyleBackColor = true
            };

            _statusLabel = new Label
            {
                Text = "Loading...",
                Size = new Size(300, 20),
                ForeColor = Color.Gray
            };

            // Timeline panels
            _detailedTimelinePanel = new Panel
            {
                Size = new Size(TIMELINE_WIDTH, TIMELINE_HEIGHT),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _consolidatedTimelinePanel = new Panel
            {
                Size = new Size(TIMELINE_WIDTH, TIMELINE_HEIGHT),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _legendPanel = new Panel
            {
                Size = new Size(250, TIMELINE_HEIGHT),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Enable custom painting
            _detailedTimelinePanel.Paint += OnDetailedTimelinePaint;
            _consolidatedTimelinePanel.Paint += OnConsolidatedTimelinePaint;
            _legendPanel.Paint += OnLegendPaint;

            // Add tooltips
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_previousDayButton, "Go to previous day");
            toolTip.SetToolTip(_nextDayButton, "Go to next day");
            toolTip.SetToolTip(_openInTimelyButton, "Upload this day's data to Timely");
            _detailedTimelinePanel.MouseMove += (s, e) => ShowTimelineTooltip(s, e, _detailedEntries, toolTip);
            _consolidatedTimelinePanel.MouseMove += (s, e) => ShowTimelineTooltip(s, e, _consolidatedEntries, toolTip);
        }

        private void LayoutControls()
        {
            var margin = 15;
            var currentY = margin;

            // Top controls
            var topPanel = new Panel
            {
                Location = new Point(margin, currentY),
                Size = new Size(this.ClientSize.Width - 2 * margin, 40),
                BackColor = Color.Transparent
            };

            var dateLabel = new Label
            {
                Text = "Date:",
                Location = new Point(0, 8),
                Size = new Size(40, 20)
            };

            _datePicker.Location = new Point(45, 5);
            _previousDayButton.Location = new Point(_datePicker.Right + 10, 5);
            _nextDayButton.Location = new Point(_previousDayButton.Right + 5, 5);
            _refreshButton.Location = new Point(_nextDayButton.Right + 10, 5);
            _openInTimelyButton.Location = new Point(_refreshButton.Right + 10, 5);
            _statusLabel.Location = new Point(_openInTimelyButton.Right + 20, 8);

            topPanel.Controls.AddRange(new Control[] { dateLabel, _datePicker, _previousDayButton, _nextDayButton, _refreshButton, _openInTimelyButton, _statusLabel });
            this.Controls.Add(topPanel);

            currentY += topPanel.Height + margin;

            // Timeline panels with headers
            var timelineY = currentY + 30;

            // Headers
            var detailedHeader = new Label
            {
                Text = "Detailed Timeline",
                Location = new Point(TIME_SCALE_WIDTH + margin, currentY),
                Size = new Size(TIMELINE_WIDTH, 25),
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var consolidatedHeader = new Label
            {
                Text = "Consolidated Timeline",
                Location = new Point(TIME_SCALE_WIDTH + margin * 2 + TIMELINE_WIDTH, currentY),
                Size = new Size(TIMELINE_WIDTH, 25),
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var legendHeader = new Label
            {
                Text = "Project Legend",
                Location = new Point(TIME_SCALE_WIDTH + margin * 3 + TIMELINE_WIDTH * 2, currentY),
                Size = new Size(250, 25),
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Timeline panels - positioned to the right of time scale
            _detailedTimelinePanel.Location = new Point(TIME_SCALE_WIDTH + margin, timelineY);
            _consolidatedTimelinePanel.Location = new Point(TIME_SCALE_WIDTH + margin * 2 + TIMELINE_WIDTH, timelineY);
            _legendPanel.Location = new Point(TIME_SCALE_WIDTH + margin * 3 + TIMELINE_WIDTH * 2, timelineY);

            this.Controls.AddRange(new Control[] {
                detailedHeader, consolidatedHeader, legendHeader,
                _detailedTimelinePanel, _consolidatedTimelinePanel, _legendPanel
            });

            // Add time scale on the left
            CreateTimeScale(timelineY);
        }

        private void CreateTimeScale(int timelineY)
        {
            var timeScalePanel = new Panel
            {
                Location = new Point(5, timelineY),
                Size = new Size(TIME_SCALE_WIDTH - 5, TIMELINE_HEIGHT),
                BackColor = Color.Transparent
            };
            timeScalePanel.Paint += OnTimeScalePaint;

            this.Controls.Add(timeScalePanel);
        }

        private void OnTimeScalePaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Transparent);

            using (var font = new Font(Font.FontFamily, 8))
            using (var brush = new SolidBrush(Color.Gray))
            {
                // Use dynamic time range if available
                var useTimeRange = _timeRangeStart != default && _timeRangeEnd != default;
                
                if (useTimeRange)
                {
                    var rangeDuration = _timeRangeEnd - _timeRangeStart;
                    var totalHours = rangeDuration.TotalHours;
                    
                    // Show time labels every hour or every 30 minutes depending on range
                    var labelInterval = totalHours > 12 ? 1.0 : 0.5; // Hours
                    
                    var currentTime = new DateTime(_timeRangeStart.Year, _timeRangeStart.Month, _timeRangeStart.Day, _timeRangeStart.Hour, 0, 0);
                    if (_timeRangeStart.Minute > 0) currentTime = currentTime.AddHours(1);

                    while (currentTime <= _timeRangeEnd)
                    {
                        var totalMinutes = (currentTime - _timeRangeStart).TotalMinutes;
                        var y = (float)(totalMinutes / rangeDuration.TotalMinutes * TIMELINE_HEIGHT);

                        if (y >= 0 && y <= TIMELINE_HEIGHT)
                        {
                            var timeText = labelInterval == 1.0 ? $"{currentTime:HH}:00" : $"{currentTime:HH}:{currentTime:mm}";
                            var textSize = e.Graphics.MeasureString(timeText, font);
                            e.Graphics.DrawString(timeText, font, brush, 
                                new PointF(TIME_SCALE_WIDTH - 10 - textSize.Width, y - textSize.Height / 2));
                        }

                        currentTime = currentTime.AddMinutes(labelInterval * 60);
                    }
                }
                else
                {
                    // Original 24-hour labels
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var y = hour * HOUR_HEIGHT;
                        var timeText = $"{hour:00}:00";
                        var textSize = e.Graphics.MeasureString(timeText, font);
                        e.Graphics.DrawString(timeText, font, brush, 
                            new PointF(TIME_SCALE_WIDTH - 10 - textSize.Width, y - textSize.Height / 2));
                    }
                }
            }
        }

        private void SetupEventHandlers()
        {
            _refreshButton.Click += OnRefreshClick;
            _datePicker.ValueChanged += OnDateChanged;
            _previousDayButton.Click += OnPreviousDayClick;
            _nextDayButton.Click += OnNextDayClick;
            _openInTimelyButton.Click += OnOpenInTimelyClick;
        }

        private async void LoadProjectColors()
        {
            try
            {
                var timelyConfig = TimelyConfiguration.Instance;
                if (string.IsNullOrEmpty(timelyConfig.CookieString) || string.IsNullOrEmpty(timelyConfig.WorkspaceId))
                {
                    // Use default colors if Timely is not configured
                    GenerateDefaultProjectColors();
                    return;
                }

                var projects = await _timelyProjectService.FetchProjectsAsync();
                foreach (var project in projects)
                {
                    if (!string.IsNullOrEmpty(project.Color))
                    {
                        try
                        {
                            var color = ColorTranslator.FromHtml(project.Color);
                            _projectColors[project.Id] = color;
                        }
                        catch
                        {
                            // Use default color if parsing fails
                            _projectColors[project.Id] = GetDefaultColorForProject(project.Id);
                        }
                    }
                    else
                    {
                        _projectColors[project.Id] = GetDefaultColorForProject(project.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading project colors: {ex.Message}");
                GenerateDefaultProjectColors();
            }
        }

        private void GenerateDefaultProjectColors()
        {
            var projectConfig = ProjectConfiguration.Instance;
            var defaultColors = new Color[]
            {
                Color.FromArgb(120, 144, 156),  // Blue Grey
                Color.FromArgb(76, 175, 80),    // Green
                Color.FromArgb(255, 152, 0),    // Orange
                Color.FromArgb(156, 39, 176),   // Purple
                Color.FromArgb(233, 30, 99),    // Pink
                Color.FromArgb(0, 150, 136),    // Teal
                Color.FromArgb(255, 87, 34),    // Deep Orange
                Color.FromArgb(103, 58, 183),   // Deep Purple
                Color.FromArgb(205, 220, 57),   // Lime
                Color.FromArgb(96, 125, 139)    // Blue Grey Dark
            };

            int colorIndex = 0;
            
            // Default project color
            _projectColors[projectConfig.DefaultProject.Id] = Color.LightGray;
            
            // Project mapping colors
            foreach (var mapping in projectConfig.ProjectMappings)
            {
                _projectColors[mapping.Project.Id] = defaultColors[colorIndex % defaultColors.Length];
                colorIndex++;
            }
        }

        private Color GetDefaultColorForProject(long projectId)
        {
            var hash = projectId.GetHashCode();
            var hue = Math.Abs(hash) % 360;
            return ColorFromHSV(hue, 0.7, 0.8);
        }

        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private void LoadDataForToday()
        {
            LoadDataForDate(_datePicker.Value.Date);
        }

        private void LoadDataForDate(DateTime date)
        {
            try
            {
                _statusLabel.Text = "Loading data...";
                _statusLabel.ForeColor = Color.Blue;

                // Get all usage data for the selected date
                var allEntries = _usageTracker.GetAllUsageHistory()
                    .Where(entry => entry.StartTime.Date == date.Date)
                    .OrderBy(entry => entry.StartTime)
                    .ToList();

                if (!allEntries.Any())
                {
                    _statusLabel.Text = $"No data found for {date:yyyy-MM-dd}";
                    _statusLabel.ForeColor = Color.Orange;
                    _detailedEntries.Clear();
                    _consolidatedEntries.Clear();
                    RefreshTimelines();
                    return;
                }

                // Ensure all entries have end times for display
                var entriesWithEndTimes = allEntries.Select(entry => new DesktopUsageEntry
                {
                    DesktopName = entry.DesktopName,
                    StartTime = entry.StartTime,
                    EndTime = entry.EndTime ?? entry.StartTime.AddMinutes(1) // Minimum 1 minute for display
                }).ToList();

                _detailedEntries = entriesWithEndTimes;
                _consolidatedEntries = _consolidationService.ConsolidateUsageEntries(entriesWithEndTimes);

                // Calculate time range for better visualization
                CalculateTimeRange(_detailedEntries.Concat(_consolidatedEntries).ToList());

                // Detect projects for all entries
                _entryProjectMappings.Clear();
                foreach (var entry in _detailedEntries.Concat(_consolidatedEntries))
                {
                    if (!_entryProjectMappings.ContainsKey(entry))
                    {
                        _entryProjectMappings[entry] = _projectDetectionService.DetectProjectForEntry(entry);
                    }
                }

                _statusLabel.Text = $"Loaded {_detailedEntries.Count} detailed entries, {_consolidatedEntries.Count} consolidated";
                _statusLabel.ForeColor = Color.Green;

                RefreshTimelines();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
        }

        private void CalculateTimeRange(List<DesktopUsageEntry> entries)
        {
            if (!entries.Any()) return;

            var startTimes = entries.Select(e => e.StartTime);
            var endTimes = entries.Where(e => e.EndTime.HasValue).Select(e => e.EndTime.Value);
            
            var earliestTime = startTimes.Min();
            var latestTime = endTimes.Any() ? endTimes.Max() : DateTime.Now;

            // Add some padding (30 minutes before and after)
            _timeRangeStart = earliestTime.AddMinutes(-30);
            _timeRangeEnd = latestTime.AddMinutes(30);

            // Calculate minutes per pixel for scaling
            var totalMinutes = (_timeRangeEnd - _timeRangeStart).TotalMinutes;
            _minutesPerPixel = totalMinutes / TIMELINE_HEIGHT;
        }

        private void RefreshTimelines()
        {
            _detailedTimelinePanel.Invalidate();
            _consolidatedTimelinePanel.Invalidate();
            _legendPanel.Invalidate();
            
            // Also refresh time scale to update labels
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel && panel.Location.X == 5) // Time scale panel
                {
                    panel.Invalidate();
                    break;
                }
            }
        }

        private void OnDetailedTimelinePaint(object sender, PaintEventArgs e)
        {
            DrawTimeline(e.Graphics, _detailedEntries, "Detailed Timeline");
        }

        private void OnConsolidatedTimelinePaint(object sender, PaintEventArgs e)
        {
            DrawTimeline(e.Graphics, _consolidatedEntries, "Consolidated Timeline");
        }

        private void DrawTimeline(Graphics g, List<DesktopUsageEntry> entries, string title)
        {
            g.Clear(Color.White);

            if (!entries.Any()) return;

            // Use dynamic time range if available, otherwise fall back to full day
            var useTimeRange = _timeRangeStart != default && _timeRangeEnd != default;
            var rangeStart = useTimeRange ? _timeRangeStart : _datePicker.Value.Date;
            var rangeEnd = useTimeRange ? _timeRangeEnd : _datePicker.Value.Date.AddDays(1);
            var rangeDuration = rangeEnd - rangeStart;

            // Draw time grid lines based on the time range
            using (var gridPen = new Pen(Color.LightGray, 1))
            {
                if (useTimeRange)
                {
                    // Draw grid lines every hour within the time range
                    var startHour = rangeStart.Hour;
                    var endHour = rangeEnd.Hour;
                    if (rangeEnd.Date > rangeStart.Date) endHour += 24;

                    for (int hour = startHour; hour <= endHour; hour++)
                    {
                        var hourTime = rangeStart.Date.AddHours(hour);
                        if (hourTime >= rangeStart && hourTime <= rangeEnd)
                        {
                            var totalMinutes = (hourTime - rangeStart).TotalMinutes;
                            var y = (float)(totalMinutes / rangeDuration.TotalMinutes * TIMELINE_HEIGHT);
                            g.DrawLine(gridPen, 0, y, TIMELINE_WIDTH - 1, y);
                        }
                    }
                }
                else
                {
                    // Original 24-hour grid
                    for (int hour = 0; hour <= 24; hour++)
                    {
                        var y = hour * HOUR_HEIGHT;
                        g.DrawLine(gridPen, 0, y, TIMELINE_WIDTH - 1, y);
                    }
                }
            }

            // Draw timeline entries
            var entryWidth = TIMELINE_WIDTH - 20;
            var entryX = 10;

            foreach (var entry in entries)
            {
                if (entry.EndTime == null) continue;

                var project = _entryProjectMappings.ContainsKey(entry) 
                    ? _entryProjectMappings[entry] 
                    : ProjectConfiguration.Instance.DefaultProject;

                var color = _projectColors.ContainsKey(project.Id) 
                    ? _projectColors[project.Id] 
                    : Color.LightGray;

                float startY, height;

                if (useTimeRange)
                {
                    // Calculate position based on dynamic time range
                    var startFromRange = (entry.StartTime - rangeStart).TotalMinutes;
                    var endFromRange = (entry.EndTime.Value - rangeStart).TotalMinutes;

                    // Skip entries outside the visible range
                    if (endFromRange < 0 || startFromRange > rangeDuration.TotalMinutes) continue;

                    // Clamp to visible range
                    startFromRange = Math.Max(0, startFromRange);
                    endFromRange = Math.Min(rangeDuration.TotalMinutes, endFromRange);

                    startY = (float)(startFromRange / rangeDuration.TotalMinutes * TIMELINE_HEIGHT);
                    height = Math.Max(2, (float)((endFromRange - startFromRange) / rangeDuration.TotalMinutes * TIMELINE_HEIGHT));
                }
                else
                {
                    // Original calculation
                    var startMinutes = (entry.StartTime - rangeStart).TotalMinutes;
                    var endMinutes = (entry.EndTime.Value - rangeStart).TotalMinutes;
                    var durationMinutes = endMinutes - startMinutes;

                    startY = (float)(startMinutes / 60.0 * HOUR_HEIGHT);
                    height = Math.Max(2, (float)(durationMinutes / 60.0 * HOUR_HEIGHT));
                }

                // Draw the timeline block
                using (var brush = new SolidBrush(color))
                {
                    var rect = new RectangleF(entryX, startY, entryWidth, height);
                    g.FillRectangle(brush, rect);
                }

                // Draw border
                using (var borderPen = new Pen(Color.Black, 1))
                {
                    var rect = new RectangleF(entryX, startY, entryWidth, height);
                    g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                // Draw text if there's enough space
                if (height > 15)
                {
                    var textRect = new RectangleF(entryX + 2, startY + 1, entryWidth - 4, height - 2);
                    using (var font = new Font("Arial", 8))
                    using (var textBrush = new SolidBrush(GetContrastColor(color)))
                    {
                        var format = new StringFormat 
                        { 
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near,
                            Trimming = StringTrimming.EllipsisCharacter
                        };
                        g.DrawString(entry.DesktopName, font, textBrush, textRect, format);
                    }
                }
            }
        }

        private void OnLegendPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(250, 250, 250));

            var y = 10;
            var minLineHeight = 25;

            using (var font = new Font("Arial", 9))
            using (var textBrush = new SolidBrush(Color.Black))
            {
                var projectInfo = new HashSet<ProjectInfo>();
                
                // Collect all unique projects from current entries
                foreach (var entry in _detailedEntries.Concat(_consolidatedEntries))
                {
                    if (_entryProjectMappings.ContainsKey(entry))
                    {
                        projectInfo.Add(_entryProjectMappings[entry]);
                    }
                }

                foreach (var project in projectInfo.OrderBy(p => p.Name))
                {
                    var color = _projectColors.ContainsKey(project.Id) 
                        ? _projectColors[project.Id] 
                        : Color.LightGray;

                    // Draw color box
                    using (var brush = new SolidBrush(color))
                    {
                        e.Graphics.FillRectangle(brush, 10, y, 15, 15);
                    }
                    e.Graphics.DrawRectangle(Pens.Black, 10, y, 15, 15);

                    // Measure text height to handle multi-line text
                    var textRect = new RectangleF(30, y, 210, 1000); // Large height for measurement
                    var textFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Near,
                        Alignment = StringAlignment.Near,
                        Trimming = StringTrimming.Word
                    };

                    var textSize = e.Graphics.MeasureString(project.Name, font, (int)textRect.Width, textFormat);
                    var actualHeight = Math.Max(minLineHeight, (int)Math.Ceiling(textSize.Height));

                    // Draw project name with proper height
                    var drawRect = new RectangleF(30, y, 210, actualHeight);
                    e.Graphics.DrawString(project.Name, font, textBrush, drawRect, textFormat);

                    y += actualHeight + 5; // Add some spacing between items
                }
            }
        }

        private Color GetContrastColor(Color background)
        {
            var brightness = (background.R * 0.299 + background.G * 0.587 + background.B * 0.114) / 255;
            return brightness > 0.5 ? Color.Black : Color.White;
        }

        private void ShowTimelineTooltip(object sender, MouseEventArgs e, List<DesktopUsageEntry> entries, ToolTip toolTip)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            DateTime timeAtMouse;

            // Use dynamic time range if available
            var useTimeRange = _timeRangeStart != default && _timeRangeEnd != default;
            if (useTimeRange)
            {
                var rangeDuration = _timeRangeEnd - _timeRangeStart;
                var mouseRatio = e.Y / (double)TIMELINE_HEIGHT;
                var minutesFromStart = mouseRatio * rangeDuration.TotalMinutes;
                timeAtMouse = _timeRangeStart.AddMinutes(minutesFromStart);
            }
            else
            {
                var startOfDay = _datePicker.Value.Date;
                var minutesFromTop = (e.Y / (double)HOUR_HEIGHT) * 60;
                timeAtMouse = startOfDay.AddMinutes(minutesFromTop);
            }

            var entry = entries.FirstOrDefault(ent => 
                ent.StartTime <= timeAtMouse && 
                (ent.EndTime ?? DateTime.Now) >= timeAtMouse);

            if (entry != null)
            {
                var project = _entryProjectMappings.ContainsKey(entry) 
                    ? _entryProjectMappings[entry] 
                    : ProjectConfiguration.Instance.DefaultProject;

                var tooltipText = $"Desktop: {entry.DesktopName}\n" +
                                  $"Project: {project.Name}\n" +
                                  $"Start: {entry.StartTime:HH:mm:ss}\n" +
                                  $"End: {entry.EndTime?.ToString("HH:mm:ss") ?? "Active"}\n" +
                                  $"Duration: {entry.Duration:hh\\:mm\\:ss}";

                toolTip.SetToolTip(panel, tooltipText);
            }
            else
            {
                toolTip.SetToolTip(panel, "");
            }
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            LoadDataForDate(_datePicker.Value.Date);
        }

        private void OnDateChanged(object sender, EventArgs e)
        {
            LoadDataForDate(_datePicker.Value.Date);
        }

        private void OnPreviousDayClick(object sender, EventArgs e)
        {
            _datePicker.Value = _datePicker.Value.AddDays(-1);
        }

        private void OnNextDayClick(object sender, EventArgs e)
        {
            _datePicker.Value = _datePicker.Value.AddDays(1);
        }

        private async void OnOpenInTimelyClick(object sender, EventArgs e)
        {
            try
            {
                // Check if Timely configuration is set up
                var timelyConfig = TimelyConfiguration.Instance;
                if (!timelyConfig.IsConfigured())
                {
                    var result = MessageBox.Show(
                        "Timely configuration is not set up. Would you like to configure it now?\n\n" +
                        "You'll need to provide:\n" +
                        "- CSRF Token (from browser network requests)\n" +
                        "- Cookie String (from browser)\n" +
                        "- Project ID and User ID (from Timely)\n\n" +
                        "The configuration file will be created at:\n" +
                        TimelyConfiguration.GetConfigFilePath(),
                        "Timely Configuration Required",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        var configForm = new TimelyConfigurationFormEnhanced();
                        if (configForm.ShowDialog() == DialogResult.OK)
                        {
                            MessageBox.Show("Timely configuration saved successfully!", "Configuration Saved", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            return; // User cancelled configuration
                        }
                    }
                    else
                    {
                        return; // User chose not to configure
                    }
                }

                // Get usage data for the selected date
                var selectedDate = _datePicker.Value.Date;
                var allEntries = _usageTracker.GetAllUsageHistory();
                
                // Check if we have any data for the selected date
                var entriesForDate = allEntries.Where(entry => entry.StartTime.Date == selectedDate).ToList();
                if (!entriesForDate.Any())
                {
                    MessageBox.Show($"No usage data available for {selectedDate:yyyy-MM-dd} to upload to Timely.",
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Confirm the upload action
                var confirmResult = MessageBox.Show(
                    $"This will upload usage data for {selectedDate:yyyy-MM-dd} directly to Timely.\n\n" +
                    $"Found {entriesForDate.Count} usage entries for this date.\n\n" +
                    "Are you sure you want to proceed?\n\n" +
                    "Note: This will create time entries in your Timely workspace.",
                    "Confirm Timely Upload",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }

                // Show progress indication
                _statusLabel.Text = $"Uploading {selectedDate:yyyy-MM-dd} to Timely...";
                _statusLabel.ForeColor = Color.Blue;
                Application.DoEvents();

                // Upload to Timely using the new date-specific overload
                using (var timelyService = new TimelyApiService())
                {
                    var uploadResult = await timelyService.UploadToTimelyAsync(allEntries, selectedDate);

                    if (uploadResult.Success)
                    {
                        var successMessage = $"Successfully uploaded {uploadResult.SuccessCount} entries for {selectedDate:yyyy-MM-dd} to Timely.";
                        _statusLabel.Text = "Upload completed successfully";
                        _statusLabel.ForeColor = Color.Green;
                        MessageBox.Show(successMessage, "Upload Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        _statusLabel.Text = "Upload failed";
                        _statusLabel.ForeColor = Color.Red;
                        
                        var errorMessage = $"No entries were successfully uploaded for {selectedDate:yyyy-MM-dd} ({uploadResult.FailureCount} failed)";
                        if (uploadResult.Errors.Any())
                        {
                            errorMessage += "\n\nErrors:\n" + string.Join("\n", uploadResult.Errors.Take(5));
                            if (uploadResult.Errors.Count > 5)
                            {
                                errorMessage += $"\n... and {uploadResult.Errors.Count - 5} more errors";
                            }
                        }

                        MessageBox.Show(errorMessage, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Offer to reconfigure Timely on error
                        var reconfigureResult = MessageBox.Show(
                            "Would you like to reconfigure Timely settings to attempt to resolve the issue?",
                            "Reconfigure Timely",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (reconfigureResult == DialogResult.Yes)
                        {
                            var configForm = new TimelyConfigurationFormEnhanced();
                            if (configForm.ShowDialog() == DialogResult.OK)
                            {
                                MessageBox.Show("Timely configuration updated successfully!", "Configuration Updated", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Upload error";
                _statusLabel.ForeColor = Color.Red;
                MessageBox.Show($"Error uploading to Timely: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timelyProjectService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
