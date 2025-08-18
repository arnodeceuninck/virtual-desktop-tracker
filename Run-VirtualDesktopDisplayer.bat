@echo off
echo Starting Virtual Desktop Displayer with Integrated Tracker...
echo.
echo Features:
echo - Shows current virtual desktop name in corner
echo - Automatically tracks and logs your desktop usage
echo - Right-click for menu (view logs, generate reports, open log folder)
echo - Double-click to exit
echo.
cd /d "%~dp0VirtualDesktopDisplayer"
start "" ".\bin\Debug\net9.0-windows\VirtualDesktopDisplayer.exe"