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
                string desktopName = GetCurrentDesktopName();
                Console.WriteLine($"Current active desktop: {desktopName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting desktop name: {ex.Message}");
                Console.WriteLine("Make sure you're running on Windows 11 24H2 and have proper permissions.");
            }
        }

        static string GetCurrentDesktopName()
        {
            try
            {
                // Try multiple possible paths for the VirtualDesktop executable
                string[] possiblePaths = {
                    // When running with dotnet run (working directory is project folder)
                    Path.Combine("..", "VirtualDesktop", "VirtualDesktop11-24H2.exe"),
                    // When running compiled exe from bin folder
                    Path.Combine("..", "..", "..", "..", "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe"),
                    // Absolute path fallback
                    @"C:\Users\ANK\repos\virtual-desktop-tracker\VirtualDesktop\VirtualDesktop11-24H2.exe"
                };

                string virtualDesktopExePath = "";
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        virtualDesktopExePath = path;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(virtualDesktopExePath))
                {
                    throw new FileNotFoundException($"VirtualDesktop executable not found. Tried paths: {string.Join(", ", possiblePaths)}");
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
