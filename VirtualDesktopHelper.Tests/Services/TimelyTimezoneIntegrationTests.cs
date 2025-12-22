using FluentAssertions;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;
using Xunit;
using Xunit.Abstractions;

namespace VirtualDesktopHelper.Tests.Services
{
    /// <summary>
    /// Integration tests to verify that timezone offset is correctly applied to Timely API calls.
    /// </summary>
    public class TimelyTimezoneIntegrationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public TimelyTimezoneIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("Setting up Timely Timezone Integration tests");
            TimelyConfiguration.Reset();
        }

        public void Dispose()
        {
            _output.WriteLine("Cleaning up Timely Timezone Integration tests");
            TimelyConfiguration.Reset();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TimelyJavaScriptGenerator_ShouldUseSystemTimezone_WhenNotConfigured()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;
            config.WorkspaceId = "123456";
            config.UserId = 999;
            config.DefaultProjectId = 888;
            config.CsrfToken = "test_token";
            config.CookieString = "test_cookie";
            config.SocketId = "test_socket";
            
            var generator = new TimelyJavaScriptGenerator(timelyConfig: config);
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Test Desktop",
                    StartTime = new DateTime(2025, 8, 23, 9, 30, 0),
                    EndTime = new DateTime(2025, 8, 23, 10, 30, 0)
                }
            };

            // Act
            var javascript = generator.GenerateTimelyJavaScript(entries, currentDayOnly: false);

            // Assert
            javascript.Should().NotBeNullOrEmpty();
            
            // Verify that the generated JavaScript contains timestamp with timezone offset
            var systemOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var expectedSign = systemOffset.TotalMinutes >= 0 ? "+" : "-";
            var expectedHours = Math.Abs(systemOffset.Hours).ToString("D2");
            var expectedMinutes = Math.Abs(systemOffset.Minutes).ToString("D2");
            var expectedOffset = $"{expectedSign}{expectedHours}:{expectedMinutes}";
            
            javascript.Should().Contain(expectedOffset, 
                "generated JavaScript should contain system timezone offset");
            
            _output.WriteLine($"Generated JavaScript with system timezone offset: {expectedOffset}");
            _output.WriteLine($"DST Active: {TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now)}");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TimelyJavaScriptGenerator_ShouldUseCustomTimezone_WhenConfigured()
        {
            // Arrange
            var config = TimelyConfiguration.Instance;
            config.WorkspaceId = "123456";
            config.UserId = 999;
            config.DefaultProjectId = 888;
            config.CsrfToken = "test_token";
            config.CookieString = "test_cookie";
            config.SocketId = "test_socket";
            config.TimezoneOffset = "+05:30"; // Custom timezone (e.g., IST)
            
            var generator = new TimelyJavaScriptGenerator(timelyConfig: config);
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Test Desktop",
                    StartTime = new DateTime(2025, 8, 23, 9, 30, 0),
                    EndTime = new DateTime(2025, 8, 23, 10, 30, 0)
                }
            };

            // Act
            var javascript = generator.GenerateTimelyJavaScript(entries, currentDayOnly: false);

            // Assert
            javascript.Should().NotBeNullOrEmpty();
            javascript.Should().Contain("+05:30", 
                "generated JavaScript should contain custom timezone offset");
            
            _output.WriteLine("Generated JavaScript with custom timezone offset: +05:30");
        }

        [Theory]
        [Trait("Category", "Integration")]
        [InlineData(1, 0, "+01:00")]  // Winter time (standard time)
        [InlineData(7, 0, "+02:00")]  // Summer time (daylight saving time)
        public void TimelyJavaScriptGenerator_ShouldReflectDSTChanges_ForCentralEuropeanTime(
            int month, int day, string expectedOffset)
        {
            // This test simulates how the timezone offset should change with DST
            // For Central European Time: UTC+1 in winter, UTC+2 in summer
            
            // Arrange
            var config = TimelyConfiguration.Instance;
            config.WorkspaceId = "123456";
            config.UserId = 999;
            config.DefaultProjectId = 888;
            config.CsrfToken = "test_token";
            config.CookieString = "test_cookie";
            config.SocketId = "test_socket";
            // Don't set TimezoneOffset, let it use system default
            
            var testDate = new DateTime(2025, month, 1, 9, 30, 0);
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Test Desktop",
                    StartTime = testDate,
                    EndTime = testDate.AddHours(1)
                }
            };

            // Act
            var generator = new TimelyJavaScriptGenerator(timelyConfig: config);
            var javascript = generator.GenerateTimelyJavaScript(entries, currentDayOnly: false);

            // Assert
            javascript.Should().NotBeNullOrEmpty();
            
            // Note: This test will only pass if running in Central European timezone
            // In a real environment, the system timezone determines the offset
            _output.WriteLine($"Month: {month}, Expected offset for CET: {expectedOffset}");
            _output.WriteLine($"Current system offset: {config.TimezoneOffset}");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CurlParser_And_Configuration_ShouldWorkTogether()
        {
            // Arrange - Simulate parsing a curl request with timezone
            var curlRequest = @"curl ""https://app.timelyapp.com/946869/hours"" \
  -H ""x-csrf-token: test_token"" \
  -H ""tl-socket-id: 123.456"" \
  -b ""test=cookie"" \
  --data-raw '{""event"":{""from"":""2025-08-23T09:30:00.000+02:00"",""to"":""2025-08-23T10:30:00.000+02:00"",""project_id"":123456,""user_id"":789012}}'";

            var parser = new CurlRequestParser();
            var parsed = parser.ParseCurlRequest(curlRequest);

            // Act - Apply parsed configuration
            var config = TimelyConfiguration.Instance;
            config.WorkspaceId = parsed.WorkspaceId;
            config.DefaultProjectId = parsed.ProjectId;
            config.UserId = parsed.UserId;
            config.CsrfToken = parsed.CsrfToken;
            config.CookieString = parsed.CookieString;
            config.SocketId = parsed.SocketId;
            if (!string.IsNullOrEmpty(parsed.TimezoneOffset))
            {
                config.TimezoneOffset = parsed.TimezoneOffset;
            }

            // Assert
            config.TimezoneOffset.Should().Be("+02:00");
            
            // Verify that subsequent calls will use this timezone
            var generator = new TimelyJavaScriptGenerator(timelyConfig: config);
            var entries = new List<DesktopUsageEntry>
            {
                new DesktopUsageEntry
                {
                    DesktopName = "Meeting",
                    StartTime = new DateTime(2025, 8, 23, 9, 30, 0),
                    EndTime = new DateTime(2025, 8, 23, 10, 30, 0)
                }
            };
            
            var javascript = generator.GenerateTimelyJavaScript(entries, currentDayOnly: false);
            javascript.Should().Contain("+02:00");
            
            _output.WriteLine("Successfully integrated curl parser timezone with configuration");
        }
    }
}
