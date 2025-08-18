# Virtual Desktop Tracker

A comprehensive Windows virtual desktop monitoring and tracking application that displays the current active desktop name and tracks your virtual desktop usage over time.

## Features

### 🖥️ Real-time Desktop Display
- Shows the current active virtual desktop name in the lower-right corner of your screen
- Updates every second to reflect desktop changes
- Semi-transparent overlay that doesn't interfere with your work
- Always stays on top for easy visibility

### 📊 Usage Tracking
- **Automatic tracking**: Records when you switch between virtual desktops
- **Desktop renaming detection**: Tracks desktop name changes
- **Time-based logging**: Records precise start and end times for each desktop session
- **Persistent storage**: Usage data is saved to JSON files in your Documents folder

### 📈 Usage Reports
- **Detailed reports**: Generate comprehensive usage reports showing:
  - Total time spent on each desktop
  - Daily usage breakdown with timeline
  - Duration formatting (hours, minutes, seconds)
- **Multiple file formats**: 
  - JSON log file for raw data (`VirtualDesktopUsage.json`)
  - Human-readable text report (`VirtualDesktopUsageReport.txt`)

### 🎛️ Interactive Controls
- **Right-click menu** with options to:
  - View usage log
  - Generate usage report
  - Open log folder
  - Exit application
- **Double-click to exit**

## Quick Start

### Running the Desktop Tracker
```batch
# Start the visual desktop tracker (recommended)
run-displayer.bat

# Or run directly
cd VirtualDesktopDisplayer
dotnet run
```

### Viewing Usage Data
```batch
# View usage statistics in console
view-usage.bat

# Open usage files in Notepad
open-usage-files.bat
```

## File Structure

```
virtual-desktop-tracker/
├── VirtualDesktopDisplayer/     # Main GUI application
├── VirtualDesktopHelper/        # Core tracking and desktop detection library
├── DesktopUsageViewer/         # Console app for viewing usage data
├── VirtualDesktopTracker/      # Simple console test app
├── VirtualDesktop/             # Original VirtualDesktop.exe binaries
├── run-displayer.bat           # Start the tracker
├── view-usage.bat              # View usage statistics
└── open-usage-files.bat        # Open usage files
```

## Usage Data Files

The application creates two files in your Documents folder:

### VirtualDesktopUsage.json
Raw tracking data in JSON format containing:
- Desktop name
- Start time (ISO format)
- End time (ISO format, null if ongoing)
- Calculated duration

### VirtualDesktopUsageReport.txt
Human-readable report containing:
- Total time per desktop (sorted by usage)
- Daily breakdown with timeline
- Session-by-session details

## Example Usage Report

```
Virtual Desktop Usage Report
Generated: 2025-08-18 21:15:03
==================================================

Total Time Per Desktop:
------------------------------
Selene ACC Upgrade B: 27s
Selene ACC Upgrade: 24s
ACC uitrol: 2s
GitHub application: 1s

Daily Usage Breakdown:
------------------------------

2025-08-18:
  21:14:06 - 21:14:17 : Selene ACC Upgrade (10s)
  21:14:17 - 21:14:19 : ACC uitrol (2s)
  21:14:19 - 21:14:21 : Selene ACC Upgrade (1s)
  21:14:21 - 21:14:23 : GitHub application (1s)
  21:14:23 - 21:14:36 : Selene ACC Upgrade (13s)
  21:14:36 - ongoing : Selene ACC Upgrade B (27s)
```

## Technical Requirements

- **Windows 11** (24H2 recommended)
- **.NET 9.0** runtime
- **Virtual Desktop feature** enabled in Windows

## How It Works

1. **Desktop Detection**: Uses the VirtualDesktop.exe utility to query current desktop information
2. **Change Detection**: Monitors desktop switches every second
3. **Session Tracking**: Records start/end times for each desktop session
4. **Data Persistence**: Saves all tracking data to JSON files
5. **Report Generation**: Creates formatted reports from tracking data

## Building from Source

```batch
# Build all components
dotnet build

# Or build individually
cd VirtualDesktopHelper && dotnet build
cd VirtualDesktopDisplayer && dotnet build
cd DesktopUsageViewer && dotnet build
```

## Privacy

- All data is stored locally on your machine
- No data is sent to external servers
- Usage logs are stored in your Documents folder
- You have full control over your tracking data
