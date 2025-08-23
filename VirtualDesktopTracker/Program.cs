using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using VirtualDesktopHelper.Services;
using VirtualDesktopHelper.Interfaces;

namespace VirtualDesktopTracker
{
	class Program
	{
		private static volatile bool _isRunning = true;
		private static string _lastDesktopName = "";

		static void Main(string[] args)
		{
			Console.WriteLine("Virtual Desktop Tracker Started");
			Console.WriteLine("Press Ctrl+C to stop tracking...");
			
			var usageTracker = VirtualDesktopServiceProvider.GetUsageTracker();
			Console.WriteLine($"Log directory: {usageTracker.GetLogDirectory()}");
			Console.WriteLine($"Current log file: {Path.GetFileName(usageTracker.GetCurrentLogFilePath())}");
			Console.WriteLine();

			// Handle Ctrl+C gracefully
			Console.CancelKeyPress += (sender, e) => {
				e.Cancel = true;
				_isRunning = false;
				Console.WriteLine("\nShutting down tracker...");
			};

			// Start tracking
			TrackDesktopChanges();

			Console.WriteLine("Tracker stopped.");
		}

		static void TrackDesktopChanges()
		{
			var usageTracker = VirtualDesktopServiceProvider.GetUsageTracker();
			var desktopNameService = VirtualDesktopServiceProvider.GetDesktopNameService();
			var screenStateDetector = VirtualDesktopServiceProvider.GetScreenStateDetector();

			while (_isRunning)
			{
				try
				{
					string currentDesktop = GetCurrentDesktopName(desktopNameService, screenStateDetector);
					
					if (!string.IsNullOrEmpty(currentDesktop) && currentDesktop != _lastDesktopName)
					{
						if (!string.IsNullOrEmpty(_lastDesktopName))
						{
							// Special handling for screen state transitions
							if (currentDesktop == "Screen Off" && _lastDesktopName != "Screen Off")
							{
								Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Screen locked/off detected - switching to low-frequency monitoring");
							}
							else if (_lastDesktopName == "Screen Off" && currentDesktop != "Screen Off")
							{
								Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Screen unlocked/on detected - resuming normal monitoring");
							}
							
							Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Desktop changed: {_lastDesktopName} -> {currentDesktop}");
						}
						else
						{
							Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Initial desktop: {currentDesktop}");
						}

						// Track the desktop usage (this automatically logs to file)
						usageTracker.TrackDesktopUsage(currentDesktop);
						_lastDesktopName = currentDesktop;
					}

					// Use different sleep intervals based on screen state
					int sleepInterval = currentDesktop == "Screen Off" ? 10000 : 2000; // 10 seconds when screen off, 2 seconds when active
					Thread.Sleep(sleepInterval);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}");
					// Check every 2 seconds even on error
					Thread.Sleep(2000);
				}
			}
		}

		static string GetCurrentDesktopName(VirtualDesktopHelper.Interfaces.IWindowsDesktopNameService desktopNameService, VirtualDesktopHelper.Interfaces.IScreenStateDetector screenStateDetector)
		{
			try
			{
				// First check if screen is locked or off
				if (screenStateDetector.IsScreenLockedOrOff())
				{
					return "Screen Off";
				}

				// Get the current desktop name using the service
				return desktopNameService.GetCurrentDesktopName();
			}
			catch
			{
				try
				{
					// Check screen state again before fallback
					if (screenStateDetector.IsScreenLockedOrOff())
					{
						return "Screen Off";
					}

					// Fallback to API method
					return GetCurrentDesktopNameUsingAPI();
				}
				catch (Exception ex)
				{
					// If we still can't get desktop name, check if screen is off
					if (screenStateDetector.IsScreenLockedOrOff())
					{
						return "Screen Off";
					}
					return $"Error: {ex.Message}";
				}
			}
		}

		static string GetCurrentDesktopNameUsingAPI()
		{
			try
			{
				// Get the current desktop index using the VirtualDesktop API
				int visibleDesktop = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);

				// Get the name using the same method as the VirtualDesktop executable's LIST command
				return VirtualDesktop.Desktop.DesktopNameFromIndex(visibleDesktop);
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get desktop name directly from API: {ex.Message}", ex);
			}
		}
	}
}
