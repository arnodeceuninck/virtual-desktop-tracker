# Virtual Desktop Tracker

A comprehensive Windows application suite for tracking and managing virtual desktop usage throughout the day. The core functionality revolves around the **VirtualDesktopDisplayer**, which provides real-time virtual desktop monitoring with automatic time tracking and project detection capabilities.

## 🚀 Core Functionality

The **VirtualDesktopDisplayer** is the main application that provides:

- **Real-time Desktop Display**: Shows the current virtual desktop name in an unobtrusive corner display that stays visible across all applications
- **Automatic Time Tracking**: Tracks virtual desktop usage with detailed timestamps, duration metrics, and generates comprehensive JSON reports
- **Smart Project Detection**: Automatically detects and maps projects based on desktop name keywords
- **One-click Desktop Renaming**: Easily modify virtual desktop names with a single click
- **Timely Integration**: Automatically sync your desktop usage data to Timely for seamless time tracking

## 📁 Project Structure

```
virtual-desktop-tracker/
├── VirtualDesktopDisplayer/          # Main GUI application
├── VirtualDesktopTracker/            # Console application for tracking
├── VirtualDesktopHelper/             # Core library with shared functionality
├── VirtualDesktopHelper.Tests/      # Unit tests
├── VirtualDesktop/                   # External dependency (MScholtes/VirtualDesktop)
├── run-displayer.bat                # Quick start script
└── Run-VirtualDesktopDisplayer.bat  # Alternative launcher
```

## 🛠️ Prerequisites

### Required Dependencies

1. **Clone the VirtualDesktop library** in the same directory:
   ```bash
   git clone https://github.com/MScholtes/VirtualDesktop.git
   ```
   Compile the VirtualDesktop project for your Windows version:
   ```bash
   cd VirtualDesktop
   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe VirtualDesktop11.cs
   ```

2. **System Requirements**:
   - Windows 10/11 with Virtual Desktop support
   - .NET 9.0 or later

### Directory Structure
After cloning, your directory should look like:
```
your-project-folder/
├── virtual-desktop-tracker/    # This repository
└── VirtualDesktop/            # MScholtes' VirtualDesktop repository
```

## 🚀 Quick Start

### Method 1: Using Batch File
```bash
# Run the main application
.\run-displayer.bat
```

### Method 2: Build and Run
```bash
# Build the solution
dotnet build

# Run the displayer
cd VirtualDesktopDisplayer
dotnet run
```

### Method 3: Run Pre-built Executable
```bash
cd VirtualDesktopDisplayer\bin\Debug\net9.0-windows
.\VirtualDesktopDisplayer.exe
```

## 💡 How to Use

1. **Start the Application**: Use any of the methods above to launch the VirtualDesktopDisplayer

2. **Desktop Display**: You'll see the current virtual desktop name appear in the bottom-right corner of your screen

3. **Automatic Tracking**: The application automatically starts tracking your desktop usage in the background

4. **Rename Desktops**: Click on the desktop name display to quickly rename your virtual desktop

5. **View Reports**: Right-click on the display for options to:
   - View usage logs
   - Generate reports
   - Open log folder
   - Configure settings
   - Exit application

6. **Exit**: Double-click the display or right-click → Exit

## 📊 Usage Reports

The application generates detailed JSON reports with information like:

```json
{
  "GeneratedAt": "2025-08-24 10:30:00",
  "TotalActivities": 15,
  "Activities": [
    {
      "DesktopName": "Development Work",
      "StartTime": "2025-08-24 09:00:00",
      "EndTime": "2025-08-24 10:15:00",
      "DurationFormatted": "1h 15m",
      "Date": "2025-08-24"
    }
  ]
}
```

Reports are automatically saved to the `VirtualDesktopLogs` directory in your Documents folder. 

## ⚙️ Configuration

### Project Detection
Configure automatic project detection by editing keywords in the project configuration:

```csharp
// Example project mapping
{
  "Project": { "Id": 12345, "Name": "My Project" },
  "Keywords": ["keyword1", "keyword2"]
}
```

### Timely Integration
Set up Timely integration for automatic time tracking by configuring:
- API credentials
- Workspace ID
- Default project mappings

## 🏗️ Architecture

### Components

- **VirtualDesktopDisplayer**: Main WinForms GUI application
- **VirtualDesktopTracker**: Console application for command-line usage
- **VirtualDesktopHelper**: Core library containing:
  - Desktop name detection
  - Usage tracking services
  - Project configuration
  - Timely integration
  - Screen state detection

### Key Services

- `IWindowsDesktopNameService`: Retrieves current desktop names
- `IDesktopUsageTracker`: Tracks and logs desktop usage
- `IScreenStateDetector`: Detects screen lock/unlock events
- `IUsageConsolidationService`: Consolidates and processes usage data

## 🧪 Testing

Run the test suite:
```bash
dotnet test
```

The test suite includes:
- Unit tests for all services
- Integration tests for desktop detection
- Performance tests for tracking accuracy

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 Dependencies

- **External**: [MScholtes/VirtualDesktop](https://github.com/MScholtes/VirtualDesktop) - Provides the core Windows virtual desktop API access
- **Internal**: .NET 9.0, Windows Forms, System.Text.Json

## 📄 License

This project is licensed under the MIT License - see the individual component licenses for details.

## 🐛 Troubleshooting

### Common Issues

**Desktop name shows as "Error: ..."**
- Ensure the VirtualDesktop folder is cloned in the correct location
- Verify that VirtualDesktop11.exe exists and is executable
