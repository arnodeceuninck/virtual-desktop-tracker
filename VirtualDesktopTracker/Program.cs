using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using VirtualDesktopHelper.Services;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Configuration;

namespace VirtualDesktopTracker
{
	class Program
	{
		private static volatile bool _isRunning = true;
		private static string _lastDesktopName = "";

		static async Task<int> Main(string[] args)
		{
			try
			{
				// Check if this is a report generation request
				if (args.Length > 0 && (args[0].Equals("report", StringComparison.OrdinalIgnoreCase) || 
				                        args[0].Equals("--report", StringComparison.OrdinalIgnoreCase) || 
				                        args[0].Equals("-r", StringComparison.OrdinalIgnoreCase)))
				{
					await GenerateReportFromArgs(args);
					return 0;
				}

				// Show help if requested
				if (args.Length > 0 && (args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) || 
				                        args[0].Equals("-h", StringComparison.OrdinalIgnoreCase) ||
				                        args[0].Equals("help", StringComparison.OrdinalIgnoreCase)))
				{
					ShowHelp();
					return 0;
				}

				// Default behavior: start tracking
				StartTracking();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return 1;
			}
		}

		static void ShowHelp()
		{
			Console.WriteLine("Virtual Desktop Tracker");
			Console.WriteLine();
			Console.WriteLine("Usage:");
			Console.WriteLine("  VirtualDesktopTracker                     Start desktop tracking");
			Console.WriteLine("  VirtualDesktopTracker report [options]    Generate usage report");
			Console.WriteLine("  VirtualDesktopTracker help                Show this help");
			Console.WriteLine();
			Console.WriteLine("Report Options:");
			Console.WriteLine("  --date <YYYY-MM-DD>           Generate report for specific date (default: today)");
			Console.WriteLine("  --consolidate <true|false>    Apply consolidation rules (default: true)");
			Console.WriteLine("  --min-duration <minutes>      Minimum activity duration to keep (default: 2.0)");
			Console.WriteLine("  --max-duration <minutes>      Max duration for custom consolidation (default: 15.0)");
			Console.WriteLine();
			Console.WriteLine("Examples:");
			Console.WriteLine("  VirtualDesktopTracker report --date 2025-08-22");
			Console.WriteLine("  VirtualDesktopTracker report --date 2025-08-22 --consolidate false");
			Console.WriteLine("  VirtualDesktopTracker report --min-duration 5.0 --max-duration 20.0");
		}

		static void StartTracking()
		{
			Console.WriteLine("Virtual Desktop Tracker Started");
			Console.WriteLine("Press Ctrl+C to stop tracking...");
			
			var usageTracker = new DesktopUsageTracker();
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

		static async Task GenerateReportFromArgs(string[] args)
		{
			DateTime targetDate = DateTime.Today;
			bool consolidate = true;
			double minDurationMinutes = 2.0;
			double maxDurationMinutes = 15.0;

			// Parse command line arguments
			for (int i = 1; i < args.Length; i++)
			{
				switch (args[i].ToLower())
				{
					case "--date":
					case "-d":
						if (i + 1 < args.Length)
						{
							if (DateTime.TryParseExact(args[i + 1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
							{
								targetDate = parsedDate;
								i++; // Skip the next argument as it's the date value
							}
							else
							{
								Console.WriteLine($"Invalid date format: {args[i + 1]}. Use YYYY-MM-DD format.");
								return;
							}
						}
						break;

					case "--consolidate":
					case "-c":
						if (i + 1 < args.Length)
						{
							if (bool.TryParse(args[i + 1], out bool consolidateValue))
							{
								consolidate = consolidateValue;
								i++; // Skip the next argument as it's the consolidate value
							}
							else
							{
								Console.WriteLine($"Invalid consolidate value: {args[i + 1]}. Use true or false.");
								return;
							}
						}
						break;

					case "--min-duration":
					case "-m":
						if (i + 1 < args.Length)
						{
							if (double.TryParse(args[i + 1], out double minDuration))
							{
								minDurationMinutes = minDuration;
								i++; // Skip the next argument as it's the duration value
							}
							else
							{
								Console.WriteLine($"Invalid min-duration value: {args[i + 1]}. Use a number of minutes.");
								return;
							}
						}
						break;

					case "--max-duration":
					case "-x":
						if (i + 1 < args.Length)
						{
							if (double.TryParse(args[i + 1], out double maxDuration))
							{
								maxDurationMinutes = maxDuration;
								i++; // Skip the next argument as it's the duration value
							}
							else
							{
								Console.WriteLine($"Invalid max-duration value: {args[i + 1]}. Use a number of minutes.");
								return;
							}
						}
						break;

					default:
						Console.WriteLine($"Unknown option: {args[i]}");
						ShowHelp();
						return;
				}
			}

			await GenerateReport(targetDate, consolidate, minDurationMinutes, maxDurationMinutes);
		}

		static async Task GenerateReport(DateTime targetDate, bool consolidate, double minDurationMinutes, double maxDurationMinutes)
		{
			try
			{
				Console.WriteLine($"Generating usage report for {targetDate:yyyy-MM-dd}");
				Console.WriteLine($"Consolidation: {(consolidate ? "Enabled" : "Disabled")}");
				if (consolidate)
				{
					Console.WriteLine($"Min duration: {minDurationMinutes} minutes");
					Console.WriteLine($"Max custom duration: {maxDurationMinutes} minutes");
				}
				Console.WriteLine();

				// Create configuration with consolidation settings
				var config = new TrackerConfiguration();
				config.EnableActivityConsolidation = consolidate;
				config.ConsolidationMinDurationMinutes = minDurationMinutes;
				config.CustomConsolidationMaxDurationMinutes = maxDurationMinutes;

				// Get the services
				var usageTracker = new DesktopUsageTracker();
				var reportGenerator = new UsageReportGenerator(config);

				// Get usage data for the specified date
				var usageHistory = usageTracker.GetAllUsageHistory()
					.Where(entry => entry.StartTime.Date == targetDate.Date)
					.ToList();
				
				if (usageHistory == null || usageHistory.Count == 0)
				{
					Console.WriteLine($"No usage data found for {targetDate:yyyy-MM-dd}");
					return;
				}

				Console.WriteLine($"Found {usageHistory.Count} usage entries for {targetDate:yyyy-MM-dd}");

				// Generate the report
				string report = reportGenerator.GenerateReport(usageHistory);
				
				Console.WriteLine();
				Console.WriteLine("=== USAGE REPORT ===");
				Console.WriteLine(report);

				// Also save to file
				string reportFileName = $"usage_report_{targetDate:yyyy-MM-dd}.txt";
				string reportPath = Path.Combine(usageTracker.GetLogDirectory(), reportFileName);
				
				await File.WriteAllTextAsync(reportPath, report);
				Console.WriteLine();
				Console.WriteLine($"Report saved to: {reportPath}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error generating report: {ex.Message}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
				}
			}
		}

		static void TrackDesktopChanges()
		{
			var config = TrackerConfiguration.Instance;
			var usageTracker = new DesktopUsageTracker();
			var screenStateDetector = new WindowsScreenStateDetector();
			var errorHandler = new VirtualDesktopErrorHandler(config);
			var desktopNameService = new WindowsDesktopNameService(screenStateDetector, errorHandler);

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

		static string GetCurrentDesktopName(IWindowsDesktopNameService desktopNameService, IScreenStateDetector screenStateDetector)
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
				// Use the external VirtualDesktop executable as fallback
				var processInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "VirtualDesktop", "VirtualDesktop11.exe"),
					Arguments = "/GetCurrentDesktop",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};

				using (var process = Process.Start(processInfo))
				{
					if (process != null)
					{
						string output = process.StandardOutput.ReadToEnd();
						process.WaitForExit();
						
						if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
						{
							return output.Trim();
						}
					}
				}

				return "Desktop 1"; // Fallback default
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get desktop name using external API: {ex.Message}", ex);
			}
		}
	}
}
