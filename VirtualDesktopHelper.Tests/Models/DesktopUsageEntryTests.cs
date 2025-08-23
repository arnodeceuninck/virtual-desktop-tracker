using FluentAssertions;
using VirtualDesktopHelper.Models;
using Xunit;

namespace VirtualDesktopHelper.Tests.Models
{
    public class DesktopUsageEntryTests
    {
        [Fact]
        public void DesktopUsageEntry_ShouldCalculateDuration_WhenEndTimeIsSet()
        {
            // Arrange
            var startTime = new DateTime(2025, 8, 23, 10, 0, 0);
            var endTime = new DateTime(2025, 8, 23, 10, 30, 0);
            var entry = new DesktopUsageEntry
            {
                DesktopName = "TestDesktop",
                StartTime = startTime,
                EndTime = endTime
            };

            // Act
            var duration = entry.Duration;

            // Assert
            duration.Should().Be(TimeSpan.FromMinutes(30));
        }

        [Fact]
        public void DesktopUsageEntry_ShouldCalculateDurationFromNow_WhenEndTimeIsNull()
        {
            // Arrange
            var startTime = DateTime.Now.AddMinutes(-15);
            var entry = new DesktopUsageEntry
            {
                DesktopName = "TestDesktop",
                StartTime = startTime,
                EndTime = null
            };

            // Act
            var duration = entry.Duration;

            // Assert
            duration.Should().BeCloseTo(TimeSpan.FromMinutes(15), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void IsActive_ShouldReturnTrue_WhenEndTimeIsNull()
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "TestDesktop",
                StartTime = DateTime.Now,
                EndTime = null
            };

            // Act & Assert
            entry.IsActive.Should().BeTrue();
        }

        [Fact]
        public void IsActive_ShouldReturnFalse_WhenEndTimeIsSet()
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "TestDesktop",
                StartTime = DateTime.Now.AddMinutes(-30),
                EndTime = DateTime.Now
            };

            // Act & Assert
            entry.IsActive.Should().BeFalse();
        }

        [Theory]
        [InlineData("Desktop 1")]
        [InlineData("Work Desktop")]
        [InlineData("")]
        public void DesktopName_ShouldBeSettable(string desktopName)
        {
            // Arrange & Act
            var entry = new DesktopUsageEntry
            {
                DesktopName = desktopName
            };

            // Assert
            entry.DesktopName.Should().Be(desktopName);
        }
    }
}
