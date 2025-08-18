@echo off
echo Opening Virtual Desktop Usage Files...
echo.

:: Try OneDrive Documents first, then fall back to local Documents
set DOCS_PATH=%USERPROFILE%\OneDrive - Bank Van Breda\Documents
if not exist "%DOCS_PATH%" (
    set DOCS_PATH=%USERPROFILE%\Documents
)

set LOG_DIR=%DOCS_PATH%\VirtualDesktopLogs

if exist "%DOCS_PATH%\VirtualDesktopUsageReport.txt" (
    echo Opening usage report...
    notepad.exe "%DOCS_PATH%\VirtualDesktopUsageReport.txt"
) else (
    echo No usage report found. Run the desktop usage viewer first to generate a report.
)

if exist "%LOG_DIR%" (
    echo.
    echo Opening log directory...
    explorer.exe "%LOG_DIR%"
) else (
    echo No log directory found. Run the desktop tracker first to generate usage data.
)

echo.
echo Log files are stored in: %LOG_DIR%
echo Each run of the tracker creates a new timestamped log file.

pause
