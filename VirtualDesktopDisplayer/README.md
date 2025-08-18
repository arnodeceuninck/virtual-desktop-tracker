# Virtual Desktop Displayer

A Windows Forms application that displays the current virtual desktop name in the bottom right corner of the screen, above the taskbar.

## Features

- **Real-time Updates**: Updates the desktop name every second
- **Unobtrusive Display**: Shows as a semi-transparent overlay in the bottom right corner
- **Easy Exit**: Double-click or right-click → Exit to close the application
- **Always On Top**: Stays visible above other windows
- **No Taskbar Icon**: Doesn't clutter the taskbar

## How It Works

The application uses the `VirtualDesktopHelper` library to call the VirtualDesktop executable via subprocess to get the current desktop name. It displays this information in a borderless, semi-transparent window positioned in the bottom right corner of the screen.

## Usage

1. Build and run the application:
   ```
   dotnet run
   ```

2. The desktop name will appear in the bottom right corner of your screen

3. To exit the application:
   - Double-click on the display
   - Right-click and select "Exit"

## Requirements

- Windows 10/11 with Virtual Desktop support
- .NET 9.0 or later
- The VirtualDesktop executable must be present in the `../VirtualDesktop/` directory

## Dependencies

- `VirtualDesktopHelper` - A helper library for retrieving desktop names
- Windows Forms

## Configuration

The display automatically positions itself in the bottom right corner with a 10-pixel margin from the edges. The text is displayed with a semi-transparent black background and white text for good visibility.
