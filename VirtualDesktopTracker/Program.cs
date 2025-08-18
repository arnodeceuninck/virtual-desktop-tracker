using System;
using VirtualDesktop;

namespace VirtualDesktopTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get the current active desktop
                var currentDesktop = Desktop.Current;
                
                // Get the desktop name
                string desktopName = Desktop.DesktopNameFromDesktop(currentDesktop);
                
                // Write the desktop name to console
                Console.WriteLine($"Current active desktop: {desktopName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting desktop name: {ex.Message}");
                Console.WriteLine("Make sure you're running on Windows 11 24H2 and have proper permissions.");
            }
        }
    }
}
