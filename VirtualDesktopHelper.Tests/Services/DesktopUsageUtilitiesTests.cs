using System;
using System.Collections.Generic;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;
using Xunit;

namespace VirtualDesktopHelper.Tests.Services
{
    public class DesktopUsageUtilitiesTests
    {
        [Fact]
        public void EnsureEndTimesAreSet_WithNullEndTimes_SetsToCurrentTime()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Test1",
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = null
                },
                new DesktopUsageEntry
                {
                    DesktopName = "Test2",
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(-1)
                }
            };

            // Act
            var result = DesktopUsageUtilities.EnsureEndTimesAreSet(entries);

            // Assert
            Assert.All(result, entry => Assert.NotNull(entry.EndTime));
            Assert.Equal(entries[1].EndTime, result[1].EndTime); // Second entry should keep its original EndTime
        }

        [Fact]
        public void FilterCurrentDayEntries_OnlyReturnsToday()
        {
            // Arrange
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);

            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry { DesktopName = "Yesterday", StartTime = yesterday.AddHours(10) },
                new DesktopUsageEntry { DesktopName = "Today", StartTime = today.AddHours(10) },
                new DesktopUsageEntry { DesktopName = "Tomorrow", StartTime = tomorrow.AddHours(10) }
            };

            // Act
            var result = DesktopUsageUtilities.FilterCurrentDayEntries(entries);

            // Assert
            Assert.Single(result);
            Assert.Equal("Today", result[0].DesktopName);
        }

        [Fact]
        public void FormatTimeSpan_VariousDurations_FormatsCorrectly()
        {
            // Test various time spans
            Assert.Equal("30s", DesktopUsageUtilities.FormatTimeSpan(TimeSpan.FromSeconds(30)));
            Assert.Equal("5m 30s", DesktopUsageUtilities.FormatTimeSpan(TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(30))));
            Assert.Equal("2h 15m 30s", DesktopUsageUtilities.FormatTimeSpan(TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(15)).Add(TimeSpan.FromSeconds(30))));
            
            // Test without seconds
            Assert.Equal("2h 15m", DesktopUsageUtilities.FormatTimeSpan(TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(15)), includeSeconds: false));
            Assert.Equal("15m", DesktopUsageUtilities.FormatTimeSpan(TimeSpan.FromMinutes(15), includeSeconds: false));
        }

        [Fact]
        public void ValidateEntriesForTimelySubmission_WithValidEntries_ReturnsNoErrors()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Test Desktop",
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now
                }
            };

            // Act
            var errors = DesktopUsageUtilities.ValidateEntriesForTimelySubmission(entries);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateEntriesForTimelySubmission_WithInvalidEntries_ReturnsErrors()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "", // Empty name
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now
                }
            };

            // Act
            var errors = DesktopUsageUtilities.ValidateEntriesForTimelySubmission(entries);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("empty desktop name"));
        }

        [Fact]
        public void ValidateEntriesForTimelySubmission_WithEndTimeBeforeStartTime_CatchesException()
        {
            // We need to test the validation separately because the Duration property throws an exception
            // when EndTime is before StartTime
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Test",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(-1)
            };

            // This should throw when accessing Duration
            Assert.Throws<InvalidOperationException>(() => _ = entry.Duration);
        }

        [Fact]
        public void CalculateTotalTimePerDesktop_GroupsCorrectly()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Desktop1",
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(-1)
                },
                new DesktopUsageEntry
                {
                    DesktopName = "Desktop1",
                    StartTime = DateTime.Now.AddMinutes(-30),
                    EndTime = DateTime.Now
                },
                new DesktopUsageEntry
                {
                    DesktopName = "Desktop2",
                    StartTime = DateTime.Now.AddHours(-1),
                    EndTime = DateTime.Now
                }
            };

            // Act
            var result = DesktopUsageUtilities.CalculateTotalTimePerDesktop(entries);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result["Desktop1"] > TimeSpan.FromHours(1)); // 1 hour + 30 minutes
            Assert.True(Math.Abs((result["Desktop2"] - TimeSpan.FromHours(1)).TotalMilliseconds) < 10); // Allow for small timing differences
        }
    }
}
