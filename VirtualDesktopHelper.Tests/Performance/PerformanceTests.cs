using FluentAssertions;
using System.Diagnostics;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;
using Xunit;
using Xunit.Abstractions;

namespace VirtualDesktopHelper.Tests.Performance
{
    /// <summary>
    /// Performance tests to ensure the system can handle expected loads.
    /// </summary>
    public class PerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ProjectDetection_ShouldBePerformant_WithManyEntries()
        {
            // Arrange
            var service = new ProjectDetectionService();
            var entries = new List<DesktopUsageEntry>();
            
            // Create 1000 test entries
            for (int i = 0; i < 1000; i++)
            {
                string desktopName = (i % 4) switch
                {
                    0 => $"simon workspace {i}",
                    1 => $"docker dev {i}",
                    2 => $"archief task {i}",
                    _ => $"random desktop {i}"
                };
                
                entries.Add(new DesktopUsageEntry 
                { 
                    DesktopName = desktopName
                });
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var results = service.DetectProjectsForEntries(entries);
            stopwatch.Stop();

            // Assert
            results.Should().HaveCount(1000);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
                "Processing 1000 entries should take less than 1 second");
            
            _output.WriteLine($"Processed {entries.Count} entries in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void DesktopUsageEntry_DurationCalculation_ShouldBePerformant()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>();
            var baseTime = new DateTime(2025, 8, 23, 10, 0, 0); // Use fixed time to avoid current time issues
            
            for (int i = 0; i < 10000; i++)
            {
                var entryStartTime = baseTime.AddSeconds(i); // Use seconds for smaller, consistent increments
                entries.Add(new DesktopUsageEntry
                {
                    StartTime = entryStartTime,
                    EndTime = i % 2 == 0 ? entryStartTime.AddMinutes(1) : null // Always add 1 minute to start time
                });
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var durations = entries.Select(e => e.Duration).ToList();
            stopwatch.Stop();

            // Assert
            durations.Should().HaveCount(10000);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
                "Duration calculations for 10000 entries should be very fast");
            
            _output.WriteLine($"Calculated durations for {entries.Count} entries in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void ProjectDetection_ShouldScaleLinearly(int entryCount)
        {
            // Arrange
            var service = new ProjectDetectionService();
            var entries = Enumerable.Range(0, entryCount)
                .Select(i => new DesktopUsageEntry { DesktopName = $"test desktop {i}" })
                .ToList();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var results = service.DetectProjectsForEntries(entries);
            stopwatch.Stop();

            // Assert
            results.Should().HaveCount(entryCount);
            var millisecondsPerEntry = (double)stopwatch.ElapsedMilliseconds / entryCount;
            millisecondsPerEntry.Should().BeLessThan(1.0, 
                "Each entry should be processed in less than 1ms on average");
            
            _output.WriteLine($"Processed {entryCount} entries in {stopwatch.ElapsedMilliseconds}ms " +
                             $"({millisecondsPerEntry:F3}ms per entry)");
        }

        [Fact]
        public void MemoryUsage_ShouldBeReasonable_WithManyEntries()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var service = new ProjectDetectionService();

            // Act - Create and process many entries
            var entries = Enumerable.Range(0, 10000)
                .Select(i => new DesktopUsageEntry 
                { 
                    DesktopName = $"memory test desktop {i}",
                    StartTime = DateTime.Now.AddMinutes(-i),
                    EndTime = DateTime.Now.AddMinutes(-i + 1)
                })
                .ToList();

            var results = service.DetectProjectsForEntries(entries);
            var memoryAfterProcessing = GC.GetTotalMemory(false);

            // Force garbage collection and measure again
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryAfterGC = GC.GetTotalMemory(true);

            // Assert
            var memoryUsed = memoryAfterProcessing - initialMemory;
            var memoryUsedAfterGC = memoryAfterGC - initialMemory;

            memoryUsed.Should().BeLessThan(50 * 1024 * 1024, // 50 MB
                "Memory usage should be reasonable for 10000 entries");

            _output.WriteLine($"Memory usage: {memoryUsed / 1024 / 1024:F2} MB before GC, " +
                             $"{memoryUsedAfterGC / 1024 / 1024:F2} MB after GC");
        }
    }
}
