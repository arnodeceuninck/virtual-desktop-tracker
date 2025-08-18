# Virtual Desktop Helper

A .NET library that provides methods to retrieve the current virtual desktop name on Windows.

## Features

- **Subprocess-based Desktop Detection**: Uses the VirtualDesktop executable to get desktop information
- **Robust Path Detection**: Automatically searches for the VirtualDesktop executable in multiple locations
- **Error Handling**: Graceful error handling with meaningful error messages

## Usage

```csharp
using VirtualDesktopHelper;

// Get the current desktop name
string desktopName = DesktopNameProvider.GetCurrentDesktopNameUsingSubprocess();
Console.WriteLine($"Current desktop: {desktopName}");
```

## Methods

### `DesktopNameProvider.GetCurrentDesktopNameUsingSubprocess()`

Returns the name of the currently visible virtual desktop by:
1. Locating the VirtualDesktop11-24H2.exe executable
2. Running it with the `/LIST` parameter
3. Parsing the output to find the desktop marked as "(visible)"
4. Returning the desktop name

**Returns**: `string` - The name of the current desktop, or an error message if something goes wrong.

## Requirements

- Windows 10/11 with Virtual Desktop support
- .NET 9.0 or later
- VirtualDesktop11-24H2.exe must be available in one of the searched paths

## Path Resolution

The library searches for the VirtualDesktop executable in the following locations:
- `../VirtualDesktop/VirtualDesktop11-24H2.exe` (relative to current directory)
- Various paths relative to the application's base directory

## Error Handling

The method returns user-friendly error messages instead of throwing exceptions, making it safe to use in UI applications where you want to display the error to the user rather than crash the application.
