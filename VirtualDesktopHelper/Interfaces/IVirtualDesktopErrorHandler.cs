using System;

namespace VirtualDesktopHelper.Interfaces
{
    /// <summary>
    /// Provides centralized error handling and retry mechanisms for virtual desktop operations.
    /// </summary>
    public interface IVirtualDesktopErrorHandler
    {
        /// <summary>
        /// Logs an error with context information
        /// </summary>
        void LogError(Exception exception, string context, object? additionalData = null);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        void LogWarning(string message, string context);

        /// <summary>
        /// Logs an informational message
        /// </summary>
        void LogInfo(string message, string context);

        /// <summary>
        /// Executes an action with automatic error handling and retry logic
        /// </summary>
        T ExecuteWithRetry<T>(Func<T> action, string operationName, int maxRetries = 3, TimeSpan? delay = null);

        /// <summary>
        /// Executes an action with automatic error handling and retry logic (void return)
        /// </summary>
        void ExecuteWithRetry(Action action, string operationName, int maxRetries = 3, TimeSpan? delay = null);

        /// <summary>
        /// Gets the last error that occurred
        /// </summary>
        Exception? GetLastError();

        /// <summary>
        /// Clears the error history
        /// </summary>
        void ClearErrors();
    }
}
