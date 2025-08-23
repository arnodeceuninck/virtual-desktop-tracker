using System;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Services;

namespace VirtualDesktopTracker
{
    class TestErrorHandling
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Testing Error Handling Infrastructure ===\n");
            
            // Test 1: ServiceFactory functionality
            Console.WriteLine("1. Testing ServiceFactory:");
            var serviceFactory = new ServiceFactory();
            var errorHandler = serviceFactory.GetErrorHandler();
            var desktopNameProvider = serviceFactory.GetDesktopNameProvider();
            var usageTracker = serviceFactory.GetUsageTracker();
            
            Console.WriteLine($"   ✓ ErrorHandler created: {errorHandler != null}");
            Console.WriteLine($"   ✓ DesktopNameProvider created: {desktopNameProvider != null}");
            Console.WriteLine($"   ✓ UsageTracker created: {usageTracker != null}");
            
            // Test 2: Error logging
            Console.WriteLine("\n2. Testing Error Logging:");
            errorHandler.LogInfo("Test info message");
            errorHandler.LogWarning("Test warning message");
            errorHandler.LogError(new Exception("Test exception"), "Test error context");
            Console.WriteLine("   ✓ Logging functions executed");
            
            // Test 3: Retry mechanism
            Console.WriteLine("\n3. Testing Retry Mechanism:");
            int attemptCount = 0;
            
            try
            {
                var result = errorHandler.ExecuteWithRetry(() =>
                {
                    attemptCount++;
                    if (attemptCount < 3)
                    {
                        throw new InvalidOperationException($"Simulated failure attempt {attemptCount}");
                    }
                    return "Success after retries!";
                });
                
                Console.WriteLine($"   ✓ Retry successful after {attemptCount} attempts: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Retry failed: {ex.Message}");
            }
            
            // Test 4: Desktop name provider with error handling
            Console.WriteLine("\n4. Testing Desktop Name Provider:");
            try
            {
                var currentDesktop = desktopNameProvider.GetCurrentDesktopName();
                Console.WriteLine($"   ✓ Current desktop name: {currentDesktop}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Failed to get desktop name: {ex.Message}");
            }
            
            // Test 5: Backward compatibility
            Console.WriteLine("\n5. Testing Backward Compatibility:");
            try
            {
                var legacyTracker = VirtualDesktopHelper.Class1.DesktopUsageTracker;
                var legacyProvider = VirtualDesktopHelper.Class1.DesktopNameProvider;
                var legacyDetector = VirtualDesktopHelper.Class1.ScreenStateDetector;
                
                Console.WriteLine($"   ✓ Legacy DesktopUsageTracker: {legacyTracker != null}");
                Console.WriteLine($"   ✓ Legacy DesktopNameProvider: {legacyProvider != null}");
                Console.WriteLine($"   ✓ Legacy ScreenStateDetector: {legacyDetector != null}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Backward compatibility issue: {ex.Message}");
            }
            
            Console.WriteLine("\n=== Error Handling Tests Complete ===");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
