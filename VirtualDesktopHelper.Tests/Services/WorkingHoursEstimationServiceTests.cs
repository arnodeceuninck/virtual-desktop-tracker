using FluentAssertions;
using System;
using System.Collections.Generic;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;
using Xunit;

namespace VirtualDesktopHelper.Tests.Services
{
    public class WorkingHoursEstimationServiceTests
    {
        private readonly WorkingHoursEstimationService _service;
        private readonly DateTime _testDate = new DateTime(2025, 8, 22);

        public WorkingHoursEstimationServiceTests()
        {
            _service = new WorkingHoursEstimationService();
        }

        private DesktopUsageEntry CreateEntry(DateTime startTime, DateTime? endTime, string desktopName = "Test Desktop")
        {
            return new DesktopUsageEntry
            {
                DesktopName = desktopName,
                StartTime = startTime,
                EndTime = endTime
            };
        }

        [Fact]
        public void EstimateWorkingHours_NoEntries_ReturnsZeroHours()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>();

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().Be(0);
            result.HoursRemaining.Should().Be(7.33, because: "7h 20m = 7.33 hours");
            result.EstimatedFinishTime.Should().BeNull();
            result.LunchBreak.Should().BeNull();
            result.Message.Should().Contain("No work activities found");
        }

        [Fact]
        public void EstimateWorkingHours_EntriesBeforeSevenAM_AreFiltered()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(2), _testDate.AddHours(4), "Early Work"), // 2 AM - 4 AM
                CreateEntry(_testDate.AddHours(5.5), _testDate.AddHours(6.5), "Early Work 2"), // 5:30 AM - 6:30 AM (should be filtered)
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(10), "Normal Work") // 8 AM - 10 AM
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().Be(2, because: "Only the 8-10 AM entry should count (2 hours)");
        }

        [Fact]
        public void EstimateWorkingHours_ExactlySevenAM_IsIncluded()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(6), _testDate.AddHours(8), "Early Work") // 6:00 AM - 8:00 AM
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().Be(2, because: "6:00 AM exactly should be included");
        }

        [Fact]
        public void EstimateWorkingHours_ScreenOffEntries_AreExcluded()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(10), "Work Desktop"),
                CreateEntry(_testDate.AddHours(10), _testDate.AddHours(11), "Screen Off"),
                CreateEntry(_testDate.AddHours(11), _testDate.AddHours(13), "Work Desktop")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().Be(4, because: "Screen Off should be excluded from working time");
        }

        [Fact]
        public void EstimateWorkingHours_LunchBreakDetection_NearTwelveThirty()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(12), "Morning Work"),
                CreateEntry(_testDate.AddHours(12), _testDate.AddHours(12.5), "Screen Off"), // 12:00-12:30 (30 min)
                CreateEntry(_testDate.AddHours(12.5), _testDate.AddHours(16), "Afternoon Work")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.LunchBreak.Should().NotBeNull();
            result.LunchBreak.StartTime.Should().Be(_testDate.AddHours(12));
            result.LunchBreak.EndTime.Should().Be(_testDate.AddHours(12.5));
            result.LunchBreak.Duration.Should().Be(TimeSpan.FromMinutes(30));
            result.TotalWorkedHours.Should().Be(7.5, because: "4h morning + 3.5h afternoon = 7.5h (lunch excluded)");
        }

        [Fact]
        public void EstimateWorkingHours_NoLunchBreak_TooShort()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(12), "Morning Work"),
                CreateEntry(_testDate.AddHours(12), _testDate.AddHours(12.25), "Screen Off"), // 15 minutes (too short)
                CreateEntry(_testDate.AddHours(12.25), _testDate.AddHours(16), "Afternoon Work")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.LunchBreak.Should().BeNull("Screen Off is only 15 minutes, less than 20 minute minimum");
        }

        [Fact]
        public void EstimateWorkingHours_NoLunchBreak_OutsideTimeWindow()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(10), "Morning Work"),
                CreateEntry(_testDate.AddHours(10), _testDate.AddHours(10.5), "Screen Off"), // 10:00-10:30 (outside 11:45-13:15 window)
                CreateEntry(_testDate.AddHours(10.5), _testDate.AddHours(16), "Work")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.LunchBreak.Should().BeNull("Screen Off is outside the lunch break time window");
        }

        [Fact]
        public void EstimateWorkingHours_LunchBreakDetection_ClosestToTwelveThirty()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(12), "Morning Work"),
                CreateEntry(_testDate.AddHours(12), _testDate.AddHours(12.5), "Screen Off"), // 12:00-12:30
                CreateEntry(_testDate.AddHours(12.5), _testDate.AddHours(13), "Brief Work"),
                CreateEntry(_testDate.AddHours(13), _testDate.AddHours(13.5), "Screen Off"), // 13:00-13:30
                CreateEntry(_testDate.AddHours(13.5), _testDate.AddHours(16), "Afternoon Work")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.LunchBreak.Should().NotBeNull();
            result.LunchBreak.StartTime.Should().Be(_testDate.AddHours(12), 
                because: "12:00-12:30 is closer to 12:30 than 13:00-13:30");
        }

        [Fact]
        public void EstimateWorkingHours_LunchBreakDetection_TwelveThirtyToThirteenThirty()
        {
            // Arrange - specific test case for 12:30-13:30 lunch break (overlaps with window)
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(12.5), "Morning Work"),
                CreateEntry(_testDate.AddHours(12.5), _testDate.AddHours(13.5), "Screen Off"), // 12:30-13:30 (60 min)
                CreateEntry(_testDate.AddHours(13.5), _testDate.AddHours(16), "Afternoon Work")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.LunchBreak.Should().NotBeNull("12:30-13:30 Screen Off should be detected as lunch break (overlaps with 11:45-13:15 window)");
            result.LunchBreak.StartTime.Should().Be(_testDate.AddHours(12.5));
            result.LunchBreak.EndTime.Should().Be(_testDate.AddHours(13.5));
            result.LunchBreak.Duration.Should().Be(TimeSpan.FromMinutes(60));
            // Total work: 4.5h morning + 2.5h afternoon = 7h (excluding 1h lunch)
            result.TotalWorkedHours.Should().Be(7, because: "4.5h morning + 2.5h afternoon = 7h (excluding 1h lunch)");
        }

        [Fact]
        public void EstimateWorkingHours_AlreadyCompletedHours_ShowsFinishTime()
        {
            // Arrange - exactly 7h 20m of work
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(15.33), "Work Desktop") // 7.33 hours
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().BeApproximately(7.33, 0.01);
            result.HoursRemaining.Should().Be(0);
            result.EstimatedFinishTime.Should().Be(_testDate.AddHours(15.33));
        }

        [Fact]
        public void EstimateWorkingHours_FixedTimeEntries_CalculatesCorrectTotals()
        {
            // Arrange - all completed work sessions
            var testDate = new DateTime(2025, 8, 24);
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(testDate.AddHours(8), testDate.AddHours(12), "Morning Work"), // 4 hours
                CreateEntry(testDate.AddHours(12.5), testDate.AddHours(13), "Brief Work"), // 30 minutes
                CreateEntry(testDate.AddHours(13), testDate.AddHours(14.25), "Afternoon Work") // 1h 15m work
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, testDate);

            // Assert - 4h + 0.5h + 1.25h = 5.75h total
            result.TotalWorkedHours.Should().BeApproximately(5.75, 0.01, because: "4h + 0.5h + 1.25h = 5.75h total"); 
            result.HoursRemaining.Should().BeApproximately(1.58, 0.1, because: "7.33 - 5.75 = 1.58 hours remaining");
        }

        [Fact]
        public void EstimateWorkingHours_MultipleDesktops_CountsAllWork()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(10), "Project A"),
                CreateEntry(_testDate.AddHours(10), _testDate.AddHours(12), "Project B"),
                CreateEntry(_testDate.AddHours(13), _testDate.AddHours(15), "Project C")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().Be(6, because: "2h + 2h + 2h = 6 hours total");
        }

        [Fact]
        public void EstimateWorkingHours_DifferentDate_FiltersCorrectly()
        {
            // Arrange
            var differentDate = _testDate.AddDays(1);
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(10), "Wrong Day"),
                CreateEntry(differentDate.AddHours(8), differentDate.AddHours(10), "Correct Day")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, differentDate);

            // Assert
            result.TotalWorkedHours.Should().Be(2, because: "Only entries from the target date should count");
        }

        [Fact]
        public void EstimateWorkingHours_EdgeCaseTimings_HandledCorrectly()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(5).AddMinutes(59), _testDate.AddHours(7), "Just Before 6 AM"), // 5:59-7:00 (should be filtered)
                CreateEntry(_testDate.AddHours(6).AddMinutes(1), _testDate.AddHours(8), "Just After 6 AM"), // 6:01-8:00 (should count)
                CreateEntry(_testDate.AddHours(11).AddMinutes(44), _testDate.AddHours(12).AddMinutes(20), "Screen Off"), // 11:44-12:20 (just outside lunch window)
                CreateEntry(_testDate.AddHours(11).AddMinutes(45), _testDate.AddHours(12).AddMinutes(21), "Screen Off") // 11:45-12:21 (just inside lunch window, 36 min)
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().BeApproximately(1.98, 0.02, because: "Only 6:01-8:00 should count (~2 hours)");
            result.LunchBreak.Should().NotBeNull("The 11:45-12:21 Screen Off should be detected as lunch");
            result.LunchBreak.Duration.Should().Be(TimeSpan.FromMinutes(36));
        }

        [Fact]
        public void EstimateWorkingHours_OverlappingEntries_HandledGracefully()
        {
            // Arrange - entries that overlap (edge case)
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(12), "Desktop A"),
                CreateEntry(_testDate.AddHours(10), _testDate.AddHours(14), "Desktop B") // overlaps with Desktop A
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            result.TotalWorkedHours.Should().Be(8, because: "Should count both entries even if overlapping (4h + 4h)");
        }

        [Theory]
        [InlineData(11, 45, 12, 30, true)]  // 11:45-12:30 (45 min) - valid lunch
        [InlineData(11, 44, 12, 30, true)]  // 11:44-12:30 - overlaps with window (starts before, ends within)
        [InlineData(11, 45, 13, 15, true)]  // 11:45-13:15 - exactly at end boundary
        [InlineData(11, 45, 13, 16, true)]  // 11:45-13:16 - overlaps with window (starts within, ends after)
        [InlineData(12, 0, 12, 19, false)]  // 12:00-12:19 (19 min) - too short
        [InlineData(12, 0, 12, 20, true)]   // 12:00-12:20 (20 min) - exactly minimum
        [InlineData(13, 14, 13, 15, false)] // 13:14-13:15 (1 min) - too short even though in window
        [InlineData(13, 10, 13, 45, true)]  // 13:10-13:45 (35 min) - overlaps with window (starts within, ends after)
        [InlineData(10, 0, 10, 30, false)]  // 10:00-10:30 - completely outside window
        [InlineData(14, 0, 14, 30, false)]  // 14:00-14:30 - completely outside window
        public void EstimateWorkingHours_LunchBreakBoundaryConditions(int startHour, int startMin, int endHour, int endMin, bool shouldDetectLunch)
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                CreateEntry(_testDate.AddHours(8), _testDate.AddHours(startHour).AddMinutes(startMin), "Morning Work"),
                CreateEntry(_testDate.AddHours(startHour).AddMinutes(startMin), _testDate.AddHours(endHour).AddMinutes(endMin), "Screen Off"),
                CreateEntry(_testDate.AddHours(endHour).AddMinutes(endMin), _testDate.AddHours(16), "Afternoon Work")
            };

            // Act
            var result = _service.EstimateWorkingHours(entries, _testDate);

            // Assert
            if (shouldDetectLunch)
            {
                result.LunchBreak.Should().NotBeNull($"Screen Off from {startHour:D2}:{startMin:D2} to {endHour:D2}:{endMin:D2} should be detected as lunch break");
            }
            else
            {
                result.LunchBreak.Should().BeNull($"Screen Off from {startHour:D2}:{startMin:D2} to {endHour:D2}:{endMin:D2} should NOT be detected as lunch break");
            }
        }
    }
}
