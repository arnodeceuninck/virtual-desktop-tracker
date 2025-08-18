# VirtualDesktopTracker

A simple console application that displays the name of the current active virtual desktop on Windows 11 24H2.

## Description

This application uses the VirtualDesktop library to interact with Windows virtual desktop APIs and displays the name of the currently active desktop.

## Requirements

- Windows 11 24H2 or later
- .NET 9.0 or later

## Usage

Simply run the application and it will display the current active desktop name:

```
dotnet run
```

Example output:
```
Current active desktop: Desktop 2
```

## How it works

The application uses the `VirtualDesktop11-24H2.cs` library which provides access to Windows virtual desktop functionality through COM interop. It specifically:

1. Gets the current active desktop using `Desktop.Current`
2. Retrieves the desktop name using `Desktop.DesktopNameFromDesktop()`
3. Displays the name to the console

If a desktop doesn't have a custom name, it will display a generic name like "Desktop 1", "Desktop 2", etc.

## Error Handling

If the application encounters any errors (such as running on an unsupported Windows version or lacking proper permissions), it will display an error message with guidance.

## Building

To build the application:

```
dotnet build
```

To run the application:

```
dotnet run
```
