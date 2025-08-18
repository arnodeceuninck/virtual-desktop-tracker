@echo off
echo Opening Virtual Desktop Usage Files...
echo.

set DOCS_PATH=%USERPROFILE%\Documents

if exist "%DOCS_PATH%\VirtualDesktopUsageReport.txt" (
    echo Opening usage report...
    notepad.exe "%DOCS_PATH%\VirtualDesktopUsageReport.txt"
) else (
    echo No usage report found. Run the desktop tracker first to generate usage data.
)

if exist "%DOCS_PATH%\VirtualDesktopUsage.json" (
    echo.
    echo Opening raw usage log...
    notepad.exe "%DOCS_PATH%\VirtualDesktopUsage.json"
) else (
    echo No usage log found. Run the desktop tracker first to generate usage data.
)

pause
