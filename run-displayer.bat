@echo off
echo Starting Virtual Desktop Displayer with Integrated Tracker...
echo This app will:
echo - Display current virtual desktop name in the corner
echo - Automatically track your desktop usage
echo - Save logs for analysis
echo - Right-click for options (view logs, generate reports, etc.)
echo - Double-click to exit
echo.
cd /d "%~dp0VirtualDesktopDisplayer"
dotnet run
pause
