using System;
using System.Diagnostics;
using System.IO;

namespace VirtualDesktopHelper
{
    public class DesktopNameProvider
    {
        public static string GetCurrentDesktopNameUsingSubprocess()
        {
            try
            {
                // Path to the original VirtualDesktop executable
                string virtualDesktopExePath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "",
                    "VirtualDesktop",
                    "VirtualDesktop11-24H2.exe"
                );

                if (!File.Exists(virtualDesktopExePath))
                {
                    // Try alternative paths
                    string[] possiblePaths = {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "VirtualDesktop", "VirtualDesktop11-24H2.exe")
                    };

                    foreach (string path in possiblePaths)
                    {
                        string fullPath = Path.GetFullPath(path);
                        if (File.Exists(fullPath))
                        {
                            virtualDesktopExePath = fullPath;
                            break;
                        }
                    }

                    if (!File.Exists(virtualDesktopExePath))
                    {
                        throw new FileNotFoundException($"VirtualDesktop executable not found. Searched paths including: {virtualDesktopExePath}");
                    }
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
                return "Unknown Desktop";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
