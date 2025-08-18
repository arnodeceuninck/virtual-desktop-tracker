# Virtual Desktop Tracker

A comprehensive Windows virtual desktop monitoring and tracking application that displays the current active desktop name and tracks your virtual desktop usage over time.

## Features

### 🖥️ Real-time Desktop Display with Integrated Tracking
- Shows the current active virtual desktop name in the lower-right corner of your screen
- **Automatic usage tracking** - tracker functionality is now built directly into the displayer
- Updates every 2 seconds to reflect desktop changes and optimize performance
- Semi-transparent overlay that doesn't interfere with your work
- Always stays on top for easy visibility

### 📊 Smart Usage Tracking
- **Intelligent change detection**: Only tracks actual desktop switches (no redundant logging)
- **Desktop renaming detection**: Tracks desktop name changes
- **Time-based logging**: Records precise start and end times for each desktop session
- **Persistent storage**: Usage data is saved to JSON files in your Documents/VirtualDesktopLogs folder
- **Session-based logging**: Each run creates a new timestamped log file, preserving all historical data
- **Real-time logging**: Changes are written to log immediately when they occur
- **Dual API support**: Uses both VirtualDesktop API and subprocess fallback for maximum reliability

### 📈 Usage Reports
- **Detailed reports**: Generate comprehensive usage reports showing:
  - Total time spent on each desktop across all sessions
  - Daily usage breakdown with timeline
  - Duration formatting (hours, minutes, seconds)
- **Historical data**: Reports include data from all previous tracking sessions
- **Multiple file formats**: 
  - Individual timestamped JSON log files for each session (`VirtualDesktopUsage_YYYY-MM-DD_HH-mm-ss.json`)
  - Human-readable text report aggregating all data (`VirtualDesktopUsageReport.txt`)

### 🎛️ Interactive Controls
- **Right-click menu** with options to:
  - View usage log
  - Generate usage report
  - Open log folder
  - Exit application
- **Double-click to exit**

## Quick Start

### Running the Desktop Tracker (Recommended)
```batch
# Start the integrated tracker and displayer (recommended)
run-displayer.bat

# Or use the direct launcher
Run-VirtualDesktopDisplayer.bat

# Or run directly from source
cd VirtualDesktopDisplayer
dotnet run
```

### Console-Only Tracking (Advanced)
```batch
# Start console-only tracking (for debugging or headless operation)
run-tracker.bat

# Or run directly
cd VirtualDesktopTracker
dotnet run
```

### Viewing Usage Data
```batch
# View usage statistics in console (includes all historical data)
view-usage.bat

# Open usage files and log directory
open-usage-files.bat
```

## File Structure

```
virtual-desktop-tracker/
├── VirtualDesktopDisplayer/     # Main GUI application with integrated tracker
│   ├── Form1.cs                 # Main form with tracker logic
│   ├── VirtualDesktop11-24H2.cs # VirtualDesktop API
│   └── VirtualDesktopDisplayer.csproj
├── VirtualDesktopHelper/        # Core tracking and desktop detection library
├── DesktopUsageViewer/         # Console app for viewing usage data
├── VirtualDesktopTracker/      # Console tracking application (standalone)
├── VirtualDesktop/             # Original VirtualDesktop.exe binaries
├── run-displayer.bat           # Start the integrated GUI tracker (recommended)
├── Run-VirtualDesktopDisplayer.bat # Alternative launcher
├── run-tracker.bat             # Start standalone console tracker
├── view-usage.bat              # View usage statistics
└── open-usage-files.bat        # Open usage files and directory
```

## How It Works

The **VirtualDesktopDisplayer** now combines both display and tracking functionality:

1. **Desktop Detection**: Uses both VirtualDesktop API and subprocess fallback for maximum reliability
2. **Intelligent Change Detection**: Only logs when desktop actually changes (eliminates duplicate entries)
3. **Real-time Display**: Shows current desktop name while tracking usage
4. **Session Tracking**: Records start/end times for each desktop session
5. **Data Persistence**: Saves all tracking data to JSON files
6. **Report Generation**: Creates formatted reports from tracking data

## Usage Data Files

The application creates files in your Documents/VirtualDesktopLogs folder:

### VirtualDesktopUsage_YYYY-MM-DD_HH-mm-ss.json
Each tracking session creates a new timestamped JSON file containing:
- Desktop name
- Start time (ISO format)
- End time (ISO format, null if ongoing)
- Calculated duration

Example: `VirtualDesktopUsage_2025-08-18_21-23-15.json`

### VirtualDesktopUsageReport.txt
Human-readable report (generated in Documents folder) containing:
- Total time per desktop across all sessions (sorted by usage)
- Daily breakdown with timeline
- Session-by-session details from all log files

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

The **VirtualDesktopDisplayer** now combines both display and tracking functionality:

1. **Desktop Detection**: Uses both VirtualDesktop API and subprocess fallback for maximum reliability
2. **Intelligent Change Detection**: Only logs when desktop actually changes (eliminates duplicate entries)
3. **Real-time Display**: Shows current desktop name while tracking usage
4. **Session Tracking**: Records start/end times for each desktop session
5. **Data Persistence**: Saves all tracking data to JSON files
6. **Report Generation**: Creates formatted reports from tracking data

## Building from Source

```batch
# Build all components
dotnet build

# Or build the main integrated application
cd VirtualDesktopDisplayer && dotnet build

# Build other components individually
cd VirtualDesktopHelper && dotnet build
cd DesktopUsageViewer && dotnet build
cd VirtualDesktopTracker && dotnet build  # standalone console version
```

## Privacy

- All data is stored locally on your machine
- No data is sent to external servers
- Usage logs are stored in your Documents folder
- You have full control over your tracking data
