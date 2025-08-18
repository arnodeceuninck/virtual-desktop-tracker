@echo off
echo Virtual Desktop Tracker (Standalone Console Version)
echo ====================================================
echo NOTE: The tracker functionality is now built into the displayer.
echo For the best experience, use run-displayer.bat instead.
echo.
echo This console version provides:
echo - Continuous tracking of virtual desktop usage
echo - Console output for debugging
echo - New timestamped log file for this session
echo - Press Ctrl+C to stop tracking
echo.
cd /d "%~dp0VirtualDesktopTracker"
dotnet run
pause
