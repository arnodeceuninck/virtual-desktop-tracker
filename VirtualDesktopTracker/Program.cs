using System;
using System.Diagnostics;
using System.IO;

namespace VirtualDesktopTracker
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string desktopName = GetCurrentDesktopNameUsingAPI();
				Console.WriteLine($"Current active desktop (Direct API): {desktopName}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting desktop name: {ex.Message}");
				Console.WriteLine("Make sure you're running on Windows 11 24H2 and have proper permissions.");
			}

			try
			{
				string desktopName = GetCurrentDesktopNameUsingSubprocess();
				Console.WriteLine($"Current active desktop (Subprocess): {desktopName}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting desktop name: {ex.Message}");
				Console.WriteLine("Make sure you're running on Windows 11 24H2 and have proper permissions.");
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

		static string GetCurrentDesktopNameUsingSubprocess()
		{
			try
			{
				// Path to the original VirtualDesktop executable - one level up from our project
				string virtualDesktopExePath = Path.Combine(
					Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "",
					"VirtualDesktop",
					"VirtualDesktop11-24H2.exe"
				);

				if (!File.Exists(virtualDesktopExePath))
				{
					throw new FileNotFoundException($"VirtualDesktop executable not found at: {virtualDesktopExePath}");
				}

				// Run the VirtualDesktop.exe /LIST command
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = virtualDesktopExePath,
					Arguments = "/LIST",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};

				using (Process process = Process.Start(startInfo))
				{
					if (process == null)
					{
						throw new InvalidOperationException("Failed to start VirtualDesktop process");
					}

					string output = process.StandardOutput.ReadToEnd();
					process.WaitForExit();

					// Parse the output to find the visible desktop
					string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

					foreach (string line in lines)
					{
						string trimmedLine = line.Trim();
						if (trimmedLine.EndsWith("(visible)"))
						{
							// Remove "(visible)" suffix and return the desktop name
							return trimmedLine.Substring(0, trimmedLine.Length - "(visible)".Length).Trim();
						}
					}
				}

				// Fallback: if we couldn't parse the output, return a default message
				return "Could not determine desktop name";
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to get desktop name via subprocess: {ex.Message}", ex);
			}
		}
	}
}
