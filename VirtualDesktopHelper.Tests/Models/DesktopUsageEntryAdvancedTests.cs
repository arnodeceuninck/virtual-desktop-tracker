using FluentAssertions;
using VirtualDesktopHelper.Models;
using Xunit;
using Xunit.Abstractions;

namespace VirtualDesktopHelper.Tests.Models
{
    /// <summary>
    /// Advanced unit tests for DesktopUsageEntry demonstrating xUnit best practices.
    /// </summary>
    public class DesktopUsageEntryAdvancedTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly DateTime _fixedDateTime;

        public DesktopUsageEntryAdvancedTests(ITestOutputHelper output)
        {
            _output = output;
            _fixedDateTime = new DateTime(2025, 8, 23, 14, 30, 0);
            _output.WriteLine($"Test started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        public void Dispose()
        {
            _output.WriteLine($"Test completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        #region Duration Calculation Tests

        [Fact]
        [Trait("Category", "Duration")]
        public void Duration_ShouldReturnZero_WhenStartTimeEqualsEndTime()
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Test Desktop",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime
            };

            // Act
            var duration = entry.Duration;

            // Assert
            duration.Should().Be(TimeSpan.Zero);
            _output.WriteLine($"Duration for same start/end time: {duration}");
        }

        [Theory]
        [Trait("Category", "Duration")]
        [InlineData(1, 0, 0)] // 1 hour
        [InlineData(0, 30, 0)] // 30 minutes
        [InlineData(0, 0, 45)] // 45 seconds
        [InlineData(2, 15, 30)] // 2 hours, 15 minutes, 30 seconds
        public void Duration_ShouldCalculateCorrectly_WithFixedTimes(int hours, int minutes, int seconds)
        {
            // Arrange
            var expectedDuration = new TimeSpan(hours, minutes, seconds);
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Test Desktop",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.Add(expectedDuration)
            };

            // Act
            var actualDuration = entry.Duration;

            // Assert
            actualDuration.Should().Be(expectedDuration);
            _output.WriteLine($"Expected: {expectedDuration}, Actual: {actualDuration}");
        }

        [Fact]
        [Trait("Category", "Duration")]
        public void Duration_ShouldThrow_WhenEndTimeIsBeforeStartTime()
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Test Desktop",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.AddMinutes(-10) // End time before start time
            };

            // Act & Assert
            var act = () => entry.Duration;
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*end time cannot be before start time*");
        }

        [Fact]
        [Trait("Category", "Duration")]
        public void Duration_ShouldCalculateFromNow_WhenEndTimeIsNull()
        {
            // Arrange
            var startTime = DateTime.Now.AddMinutes(-5);
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Active Desktop",
                StartTime = startTime,
                EndTime = null
            };

            // Act
            var duration = entry.Duration;

            // Assert
            duration.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(2));
            _output.WriteLine($"Duration from start to now: {duration}");
        }

        #endregion

        #region IsActive Property Tests

        [Theory]
        [Trait("Category", "State")]
        [InlineData(null, true)]
        [InlineData("2025-08-23T15:00:00", false)]
        public void IsActive_ShouldReturnCorrectValue_BasedOnEndTime(string? endTimeString, bool expectedIsActive)
        {
            // Arrange
            DateTime? endTime = endTimeString != null ? DateTime.Parse(endTimeString) : null;
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Test Desktop",
                StartTime = _fixedDateTime,
                EndTime = endTime
            };

            // Act
            var isActive = entry.IsActive;

            // Assert
            isActive.Should().Be(expectedIsActive);
            _output.WriteLine($"EndTime: {endTime?.ToString() ?? "null"}, IsActive: {isActive}");
        }

        #endregion

        #region Property Validation Tests

        [Theory]
        [Trait("Category", "Validation")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Valid Desktop Name")]
        [InlineData("Desktop-With-Special-Characters_123")]
        public void DesktopName_ShouldAcceptVariousValues(string desktopName)
        {
            // Arrange & Act
            var entry = new DesktopUsageEntry
            {
                DesktopName = desktopName,
                StartTime = _fixedDateTime
            };

            // Assert
            entry.DesktopName.Should().Be(desktopName);
            _output.WriteLine($"Desktop name set to: '{desktopName}'");
        }

        [Fact]
        [Trait("Category", "Validation")]
        public void DesktopName_ShouldHandleNullValue()
        {
            // Arrange & Act
            var entry = new DesktopUsageEntry
            {
                DesktopName = null!,
                StartTime = _fixedDateTime
            };

            // Assert
            entry.DesktopName.Should().BeNull();
        }

        #endregion

        #region Equality and HashCode Tests

        [Fact]
        [Trait("Category", "Equality")]
        public void Equals_ShouldReturnTrue_ForIdenticalEntries()
        {
            // Arrange
            var entry1 = new DesktopUsageEntry
            {
                DesktopName = "Test Desktop",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.AddMinutes(30)
            };

            var entry2 = new DesktopUsageEntry
            {
                DesktopName = "Test Desktop",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.AddMinutes(30)
            };

            // Act & Assert
            entry1.Should().BeEquivalentTo(entry2);
            _output.WriteLine("Entries are equivalent");
        }

        [Fact]
        [Trait("Category", "Equality")]
        public void Equals_ShouldReturnFalse_ForDifferentEntries()
        {
            // Arrange
            var entry1 = new DesktopUsageEntry
            {
                DesktopName = "Desktop 1",
                StartTime = _fixedDateTime
            };

            var entry2 = new DesktopUsageEntry
            {
                DesktopName = "Desktop 2",
                StartTime = _fixedDateTime
            };

            // Act & Assert
            entry1.Should().NotBeEquivalentTo(entry2);
            _output.WriteLine("Entries are different");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void DesktopUsageEntry_ShouldHandleMinDateTime()
        {
            // Arrange & Act
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Min DateTime Test",
                StartTime = DateTime.MinValue,
                EndTime = DateTime.MinValue.AddSeconds(1)
            };

            // Assert
            entry.Duration.Should().Be(TimeSpan.FromSeconds(1));
            entry.IsActive.Should().BeFalse();
            _output.WriteLine($"Min DateTime test passed: {entry.StartTime}");
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void DesktopUsageEntry_ShouldHandleMaxDateTime()
        {
            // Arrange & Act
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Max DateTime Test",
                StartTime = DateTime.MaxValue.AddSeconds(-1),
                EndTime = DateTime.MaxValue
            };

            // Assert
            entry.Duration.Should().Be(TimeSpan.FromSeconds(1));
            entry.IsActive.Should().BeFalse();
            _output.WriteLine($"Max DateTime test passed: {entry.EndTime}");
        }

        [Theory]
        [Trait("Category", "EdgeCase")]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(10000)]
        public void Duration_ShouldHandleLargeDurations_InMilliseconds(int milliseconds)
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Large Duration Test",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.AddMilliseconds(milliseconds)
            };

            // Act
            var duration = entry.Duration;

            // Assert
            duration.Should().Be(TimeSpan.FromMilliseconds(milliseconds));
            _output.WriteLine($"Duration for {milliseconds}ms: {duration}");
        }

        #endregion

        #region Performance Tests

        [Fact]
        [Trait("Category", "Performance")]
        public void Duration_ShouldPerformWell_WithManyCalculations()
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Performance Test",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.AddHours(1)
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 100000; i++)
            {
                _ = entry.Duration;
            }

            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
            _output.WriteLine($"100,000 duration calculations completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Custom Assertions

        [Fact]
        [Trait("Category", "Custom")]
        public void DesktopUsageEntry_ShouldSatisfyBusinessRules()
        {
            // Arrange
            var entry = new DesktopUsageEntry
            {
                DesktopName = "Business Rule Test",
                StartTime = _fixedDateTime,
                EndTime = _fixedDateTime.AddMinutes(30)
            };

            // Act & Assert - Custom business rule validations
            entry.DesktopName.Should().NotBeNullOrEmpty();
            entry.StartTime.Should().NotBe(default);
            if (entry.IsActive)
            {
                entry.EndTime.Should().BeNull();
            }
            entry.Duration.Should().BeGreaterOrEqualTo(TimeSpan.Zero);

            _output.WriteLine("All business rules satisfied");
        }

        #endregion
    }

    /// <summary>
    /// Collection fixture for sharing test data across multiple test classes.
    /// </summary>
    [CollectionDefinition("Desktop Usage Collection")]
    public class DesktopUsageCollectionFixture : ICollectionFixture<DesktopUsageTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    /// <summary>
    /// Test fixture for sharing expensive setup across tests.
    /// </summary>
    public class DesktopUsageTestFixture : IDisposable
    {
        public List<DesktopUsageEntry> SampleEntries { get; }

        public DesktopUsageTestFixture()
        {
            // Expensive setup that should be shared across tests
            SampleEntries = GenerateSampleData();
        }

        private List<DesktopUsageEntry> GenerateSampleData()
        {
            var baseTime = new DateTime(2025, 8, 23, 9, 0, 0);
            return new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Development",
                    StartTime = baseTime,
                    EndTime = baseTime.AddHours(2)
                },
                new DesktopUsageEntry
                {
                    DesktopName = "Testing",
                    StartTime = baseTime.AddHours(2),
                    EndTime = baseTime.AddHours(3)
                },
                new DesktopUsageEntry
                {
                    DesktopName = "Documentation",
                    StartTime = baseTime.AddHours(3),
                    EndTime = baseTime.AddHours(4)
                }
            };
        }

        public void Dispose()
        {
            // Cleanup shared resources
            SampleEntries.Clear();
        }
    }

    /// <summary>
    /// Tests using the collection fixture.
    /// </summary>
    [Collection("Desktop Usage Collection")]
    public class DesktopUsageCollectionTests
    {
        private readonly DesktopUsageTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public DesktopUsageCollectionTests(DesktopUsageTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        [Trait("Category", "Collection")]
        public void SampleEntries_ShouldHaveCorrectCount()
        {
            // Act & Assert
            _fixture.SampleEntries.Should().HaveCount(3);
            _output.WriteLine($"Sample entries count: {_fixture.SampleEntries.Count}");
        }

        [Fact]
        [Trait("Category", "Collection")]
        public void SampleEntries_ShouldAllHaveValidDurations()
        {
            // Act & Assert
            _fixture.SampleEntries.Should().OnlyContain(entry => entry.Duration > TimeSpan.Zero);
            _output.WriteLine("All sample entries have valid durations");
        }
    }
}
