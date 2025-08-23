using System;
using System.Runtime.InteropServices;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;

namespace VirtualDesktopHelper.Services
{
    /// <summary>
    /// Windows-specific implementation for detecting screen and system state.
    /// </summary>
    public class WindowsScreenStateDetector : IScreenStateDetector
    {
        private readonly TrackerConfiguration _config;

        // Windows API imports
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        private static extern IntPtr OpenDesktop(string hDesktop, int Flags, bool Inherit, uint DesiredAccess);

        [DllImport("user32.dll")]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        private const uint SPI_GETSCREENSAVERRUNNING = 0x0072;

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        public WindowsScreenStateDetector(TrackerConfiguration? config = null)
        {
            _config = config ?? TrackerConfiguration.Instance;
        }

        public bool IsScreenLocked()
        {
            try
            {
                // Method 1: Check if screensaver is running
                if (IsScreenSaverRunning())
                    return true;

                // Method 2: Check if the current foreground window is the lock screen
                if (IsLockScreenActive())
                    return true;

                // Method 3: Try to open the current desktop
                return !CanAccessCurrentDesktop();
            }
            catch
            {
                // If we can't determine the state, assume it's not locked
                return false;
            }
        }

        public bool IsScreenOff()
        {
            try
            {
                LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
                lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);

                if (GetLastInputInfo(ref lastInputInfo))
                {
                    uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
                    return idleTime > _config.ScreenOffIdleThreshold.TotalMilliseconds;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsScreenLockedOrOff()
        {
            return IsScreenLocked() || IsScreenOff();
        }

        private bool IsScreenSaverRunning()
        {
            bool isScreenSaverRunning = false;
            SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isScreenSaverRunning, 0);
            return isScreenSaverRunning;
        }

        private bool IsLockScreenActive()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;

            var className = new System.Text.StringBuilder(256);
            GetClassName(foregroundWindow, className, className.Capacity);
            string classNameString = className.ToString();

            // Check for Windows lock screen class names
            if (classNameString.Contains("LockApp") ||
                classNameString.Contains("Windows.UI.Core.CoreWindow") ||
                classNameString.Contains("ApplicationFrame"))
            {
                var windowText = new System.Text.StringBuilder(256);
                GetWindowText(foregroundWindow, windowText, windowText.Capacity);
                string windowTitle = windowText.ToString();

                // Additional check for lock screen window titles
                return string.IsNullOrEmpty(windowTitle) ||
                       windowTitle.Contains("Lock") ||
                       windowTitle.Contains("Sign in");
            }

            return false;
        }

        private bool CanAccessCurrentDesktop()
        {
            IntPtr hDesktop = OpenDesktop("default", 0, false, 0x0100);
            if (hDesktop == IntPtr.Zero)
                return false;

            CloseDesktop(hDesktop);
            return true;
        }
    }
}
