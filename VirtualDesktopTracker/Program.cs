using System;
using System.Diagnostics;
using System.IO;
using VirtualDesktopHelper;

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
				string desktopName = DesktopNameProvider.GetCurrentDesktopNameUsingSubprocess();
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
	}
}
