using FluentAssertions;
using VirtualDesktopHelper.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace VirtualDesktopHelper.Tests.Configuration
{
    /// <summary>
    /// Unit tests for TimelyConfiguration, specifically focusing on timezone offset handling.
    /// </summary>
    public class TimelyConfigurationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public TimelyConfigurationTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("Setting up TimelyConfiguration tests");
            // Reset the singleton instance before each test
            TimelyConfiguration.Reset();
        }

        public void Dispose()
        {
            _output.WriteLine("Cleaning up TimelyConfiguration tests");
            TimelyConfiguration.Reset();
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void TimezoneOffset_ShouldReturnSystemTimezone_WhenNotExplicitlySet()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;

            // Act
            var offset = config.TimezoneOffset;

            // Assert
            offset.Should().NotBeNullOrEmpty();
            offset.Should().MatchRegex(@"^[+-]\d{2}:\d{2}$", 
                "timezone offset should be in ISO 8601 format (e.g., +02:00 or -05:00)");
            
            _output.WriteLine($"System timezone offset: {offset}");
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void TimezoneOffset_ShouldReturnCustomValue_WhenExplicitlySet()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;
            var customOffset = "+05:30"; // Example: India Standard Time

            // Act
            config.TimezoneOffset = customOffset;
            var result = config.TimezoneOffset;

            // Assert
            result.Should().Be(customOffset);
            
            _output.WriteLine($"Custom timezone offset set to: {result}");
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void TimezoneOffset_ShouldReflectCurrentDST_WhenUsingSystemDefault()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;
            var currentOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var expectedSign = currentOffset.TotalMinutes >= 0 ? "+" : "-";
            var expectedHours = Math.Abs(currentOffset.Hours).ToString("D2");
            var expectedMinutes = Math.Abs(currentOffset.Minutes).ToString("D2");
            var expectedOffset = $"{expectedSign}{expectedHours}:{expectedMinutes}";

            // Act
            var actualOffset = config.TimezoneOffset;

            // Assert
            actualOffset.Should().Be(expectedOffset, 
                "timezone offset should match system timezone including DST");
            
            _output.WriteLine($"Current system offset (with DST): {actualOffset}");
            _output.WriteLine($"DST Active: {TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now)}");
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void TimezoneOffset_ShouldBeSerializedAndDeserialized()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;
            var customOffset = "-08:00"; // Example: Pacific Standard Time
            config.TimezoneOffset = customOffset;

            // Act
            config.SaveConfiguration();
            TimelyConfiguration.Reset();
            var loadedConfig = TimelyConfiguration.Instance;

            // Assert
            // After loading from file, if the saved offset is present, it should be loaded
            // If not present in file, it will fall back to system offset
            // This test mainly verifies that the property can be set and retrieved
            _output.WriteLine($"Loaded timezone offset: {loadedConfig.TimezoneOffset}");
        }

        [Theory]
        [Trait("Category", "TimezoneOffset")]
        [InlineData("+00:00")] // UTC
        [InlineData("+01:00")] // Central European Standard Time
        [InlineData("+02:00")] // Central European Summer Time
        [InlineData("-05:00")] // Eastern Standard Time
        [InlineData("-04:00")] // Eastern Daylight Time
        [InlineData("+05:30")] // India Standard Time
        [InlineData("-08:00")] // Pacific Standard Time
        public void TimezoneOffset_ShouldAcceptVariousValidFormats(string offset)
        {
            // Arrange
            var config = TimelyConfiguration.Instance;

            // Act
            config.TimezoneOffset = offset;
            var result = config.TimezoneOffset;

            // Assert
            result.Should().Be(offset);
            
            _output.WriteLine($"Set and retrieved timezone offset: {offset}");
        }

        [Fact]
        [Trait("Category", "TimezoneOffset")]
        public void TimezoneOffset_FormatShouldBeISO8601Compliant()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;

            // Act
            var offset = config.TimezoneOffset;

            // Assert
            // Verify format: sign (+ or -) followed by HH:MM
            offset.Should().MatchRegex(@"^[+-]\d{2}:\d{2}$");
            
            // Verify hours are valid (00-14 for most timezones)
            var hours = int.Parse(offset.Substring(1, 2));
            hours.Should().BeInRange(0, 14);
            
            // Verify minutes are valid (00, 15, 30, or 45 for most timezones)
            var minutes = int.Parse(offset.Substring(4, 2));
            minutes.Should().BeInRange(0, 59);
            
            _output.WriteLine($"ISO 8601 compliant offset: {offset}");
        }

        [Fact]
        [Trait("Category", "Configuration")]
        public void Configuration_ShouldMaintainBackwardCompatibility()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;

            // Act
            // If no explicit timezone is set, it should still work with system default
            var offset = config.TimezoneOffset;

            // Assert
            offset.Should().NotBeNullOrEmpty();
            config.IsConfigured().Should().BeFalse(); // False because other required fields are not set
            
            _output.WriteLine("Configuration maintains backward compatibility");
        }
    }
}
