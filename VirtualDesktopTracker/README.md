# VirtualDesktopTracker

A simple console application that displays the name of the current active virtual desktop on Windows 11 24H2.

## Description

This application uses the original VirtualDesktop executable as a subprocess to reliably get the current active desktop name, avoiding marshaling issues that can occur with newer .NET runtimes.

## Requirements

- Windows 11 24H2 or later
- .NET 9.0 or later
- The VirtualDesktop11-24H2.exe executable (automatically compiled when needed)

## Usage

Simply run the application and it will display the current active desktop name:

```
dotnet run
```

Example output:
```
Current active desktop: Selene ACC Upgrade
```

## How it works

The application uses a subprocess approach for maximum reliability:

1. Calls the original `VirtualDesktop11-24H2.exe /LIST` command as a subprocess
2. Parses the output to find the line marked with "(visible)"
3. Extracts and displays the desktop name

This approach avoids COM marshaling issues that can occur when using the VirtualDesktop library directly in newer .NET runtimes, while still providing accurate results.

## Example output from /LIST command:
```
Virtual desktops:
-----------------
GitHub application
Selene ACC Upgrade (visible)
ACC uitrol
SdWorx
Publish python packages
ADF trigger staatsblad

Count of desktops: 6
```

## Error Handling

If the application encounters any errors (such as the VirtualDesktop executable not being found or subprocess failures), it will display an error message with guidance.

## Building

To build the application:

```
dotnet build
```

To run the application:

```
dotnet run
```

## Dependencies

The application requires the VirtualDesktop11-24H2.exe to be present in the adjacent VirtualDesktop folder. If it's not found, compile it first using the provided Compile.bat in the VirtualDesktop directory.
