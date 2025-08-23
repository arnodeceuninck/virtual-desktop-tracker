using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopHelper
{
    /// <summary>
    /// Central service provider for all virtual desktop services.
    /// Provides clean dependency injection and service management.
    /// </summary>
    public static class VirtualDesktopServiceProvider
    {
        private static IVirtualDesktopErrorHandler? _errorHandler;
        private static IWindowsDesktopNameService? _desktopNameService;
        private static IDesktopUsageTracker? _usageTracker;
        private static IScreenStateDetector? _screenStateDetector;

        /// <summary>
        /// Gets the error handler service with retry mechanisms and logging.
        /// </summary>
        public static IVirtualDesktopErrorHandler GetErrorHandler()
        {
            return _errorHandler ??= new VirtualDesktopErrorHandler(TrackerConfiguration.Instance);
        }

        /// <summary>
        /// Gets the Windows desktop name service for virtual desktop operations.
        /// </summary>
        public static IWindowsDesktopNameService GetDesktopNameService()
        {
            return _desktopNameService ??= new WindowsDesktopNameService(
                GetScreenStateDetector(), 
                GetErrorHandler());
        }

        /// <summary>
        /// Gets the desktop usage tracking service.
        /// </summary>
        public static IDesktopUsageTracker GetUsageTracker()
        {
            return _usageTracker ??= new DesktopUsageTracker();
        }

        /// <summary>
        /// Gets the screen state detection service.
        /// </summary>
        public static IScreenStateDetector GetScreenStateDetector()
        {
            return _screenStateDetector ??= new WindowsScreenStateDetector();
        }

        /// <summary>
        /// Resets all cached services. Useful for testing or configuration changes.
        /// </summary>
        public static void ResetServices()
        {
            _errorHandler = null;
            _desktopNameService = null;
            _usageTracker = null;
            _screenStateDetector = null;
        }
    }
}
