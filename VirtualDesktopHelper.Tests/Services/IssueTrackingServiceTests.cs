using FluentAssertions;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Services;
using Xunit;

namespace VirtualDesktopHelper.Tests.Services
{
    /// <summary>
    /// Unit tests for IssueTrackingService.
    /// </summary>
    public class IssueTrackingServiceTests
    {
        private readonly TrackerConfiguration _config;
        private readonly IssueTrackingService _service;

        public IssueTrackingServiceTests()
        {
            _config = new TrackerConfiguration
            {
                EnableIssueTracking = true,
                IssueFormatRegex = @"\b[A-Z][A-Z0-9]+-\d+\b",
                IssueUrlTemplate = "https://jira.company.com/browse/{0}"
            };
            _service = new IssueTrackingService(_config);
        }

        [Theory]
        [InlineData("Working on APP-5482 bug fix", "APP-5482")]
        [InlineData("PROJ-123: New feature implementation", "PROJ-123")]
        [InlineData("Fix for TICKET-9999 and testing", "TICKET-9999")]
        [InlineData("ABC123-456 edge case", "ABC123-456")]
        public void ExtractIssueFromDesktopName_ShouldReturnFirstMatch_WhenValidIssueFound(string desktopName, string expectedIssue)
        {
            // Act
            var result = _service.ExtractIssueFromDesktopName(desktopName);

            // Assert
            result.Should().Be(expectedIssue);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("No issue here")]
        [InlineData("app-123")] // lowercase, should not match default pattern
        [InlineData("A-123")] // only one letter before dash
        [InlineData("APP-")] // no number after dash
        [InlineData("123-APP")] // number before dash
        public void ExtractIssueFromDesktopName_ShouldReturnNull_WhenNoValidIssueFound(string desktopName)
        {
            // Act
            var result = _service.ExtractIssueFromDesktopName(desktopName);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractIssueFromDesktopName_ShouldReturnNull_WhenIssueTrackingDisabled()
        {
            // Arrange
            _config.EnableIssueTracking = false;

            // Act
            var result = _service.ExtractIssueFromDesktopName("APP-5482 bug fix");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractIssueFromDesktopName_ShouldReturnNull_WhenRegexPatternEmpty()
        {
            // Arrange
            _config.IssueFormatRegex = "";

            // Act
            var result = _service.ExtractIssueFromDesktopName("APP-5482 bug fix");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ExtractIssueFromDesktopName_ShouldReturnNull_WhenRegexPatternInvalid()
        {
            // Arrange
            _config.IssueFormatRegex = "[invalid regex (";

            // Act
            var result = _service.ExtractIssueFromDesktopName("APP-5482 bug fix");

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("APP-5482", "https://jira.company.com/browse/APP-5482")]
        [InlineData("PROJ-123", "https://jira.company.com/browse/PROJ-123")]
        [InlineData("TICKET-9999", "https://jira.company.com/browse/TICKET-9999")]
        public void GenerateIssueUrl_ShouldReturnCorrectUrl_WhenValidIssueProvided(string issueId, string expectedUrl)
        {
            // Act
            var result = _service.GenerateIssueUrl(issueId);

            // Assert
            result.Should().Be(expectedUrl);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void GenerateIssueUrl_ShouldReturnNull_WhenIssueIdInvalid(string issueId)
        {
            // Act
            var result = _service.GenerateIssueUrl(issueId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GenerateIssueUrl_ShouldReturnNull_WhenIssueTrackingDisabled()
        {
            // Arrange
            _config.EnableIssueTracking = false;

            // Act
            var result = _service.GenerateIssueUrl("APP-5482");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GenerateIssueUrl_ShouldReturnNull_WhenUrlTemplateEmpty()
        {
            // Arrange
            _config.IssueUrlTemplate = "";

            // Act
            var result = _service.GenerateIssueUrl("APP-5482");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GenerateIssueUrl_ShouldReturnNull_WhenUrlTemplateInvalid()
        {
            // Arrange
            _config.IssueUrlTemplate = "invalid template {0} {1} {2}"; // too many placeholders

            // Act
            var result = _service.GenerateIssueUrl("APP-5482");

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Working on APP-5482 bug fix", "https://jira.company.com/browse/APP-5482")]
        [InlineData("PROJ-123: Feature implementation", "https://jira.company.com/browse/PROJ-123")]
        public void GetIssueUrlFromDesktopName_ShouldReturnCorrectUrl_WhenValidDesktopName(string desktopName, string expectedUrl)
        {
            // Act
            var result = _service.GetIssueUrlFromDesktopName(desktopName);

            // Assert
            result.Should().Be(expectedUrl);
        }

        [Theory]
        [InlineData("No issue here")]
        [InlineData("")]
        public void GetIssueUrlFromDesktopName_ShouldReturnNull_WhenNoIssueFound(string desktopName)
        {
            // Act
            var result = _service.GetIssueUrlFromDesktopName(desktopName);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void IsConfigured_ShouldReturnTrue_WhenProperlyConfigured()
        {
            // Act
            var result = _service.IsConfigured();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsConfigured_ShouldReturnFalse_WhenIssueTrackingDisabled()
        {
            // Arrange
            _config.EnableIssueTracking = false;

            // Act
            var result = _service.IsConfigured();

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsConfigured_ShouldReturnFalse_WhenRegexPatternInvalid(string regexPattern)
        {
            // Arrange
            _config.IssueFormatRegex = regexPattern;

            // Act
            var result = _service.IsConfigured();

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void IsConfigured_ShouldReturnFalse_WhenUrlTemplateInvalid(string urlTemplate)
        {
            // Arrange
            _config.IssueUrlTemplate = urlTemplate;

            // Act
            var result = _service.IsConfigured();

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(@"\b[A-Z][A-Z0-9]+-\d+\b")]
        [InlineData(@"#\d+")]
        [InlineData(@"[A-Z]+-\d+")]
        [InlineData(@"\d+")]
        public void IsValidRegexPattern_ShouldReturnTrue_WhenPatternValid(string pattern)
        {
            // Act
            var result = IssueTrackingService.IsValidRegexPattern(pattern);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("[invalid regex (")]
        [InlineData("*invalid")]
        public void IsValidRegexPattern_ShouldReturnFalse_WhenPatternInvalid(string pattern)
        {
            // Act
            var result = IssueTrackingService.IsValidRegexPattern(pattern);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("https://jira.company.com/browse/{0}")]
        [InlineData("https://github.com/repo/issues/{0}")]
        [InlineData("http://localhost:8080/issue/{0}")]
        public void IsValidUrlTemplate_ShouldReturnTrue_WhenTemplateValid(string template)
        {
            // Act
            var result = IssueTrackingService.IsValidUrlTemplate(template);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("not-a-url/{0}")]
        [InlineData("https://invalid url with spaces/{0}")]
        [InlineData("invalid-scheme://test.com/{0}")]
        public void IsValidUrlTemplate_ShouldReturnFalse_WhenTemplateInvalid(string template)
        {
            // Act
            var result = IssueTrackingService.IsValidUrlTemplate(template);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ExtractIssueFromDesktopName_WithGitHubIssuePattern_ShouldWork()
        {
            // Arrange
            var config = new TrackerConfiguration
            {
                EnableIssueTracking = true,
                IssueFormatRegex = @"#\d+",
                IssueUrlTemplate = "https://github.com/repo/issues/{0}"
            };
            var service = new IssueTrackingService(config);

            // Act
            var result = service.ExtractIssueFromDesktopName("Fix for issue #123 in the code");

            // Assert
            result.Should().Be("#123");
        }

        [Fact]
        public void GetIssueUrlFromDesktopName_WithGitHubIssuePattern_ShouldWork()
        {
            // Arrange
            var config = new TrackerConfiguration
            {
                EnableIssueTracking = true,
                IssueFormatRegex = @"#\d+",
                IssueUrlTemplate = "https://github.com/repo/issues/{0}"
            };
            var service = new IssueTrackingService(config);

            // Act
            var result = service.GetIssueUrlFromDesktopName("Fix for issue #123 in the code");

            // Assert
            result.Should().Be("https://github.com/repo/issues/#123");
        }

        [Fact]
        public void ExtractIssueFromDesktopName_ShouldReturnFirstMatch_WhenMultipleIssuesFound()
        {
            // Act
            var result = _service.ExtractIssueFromDesktopName("Working on APP-5482 and PROJ-123");

            // Assert
            result.Should().Be("APP-5482");
        }
    }
}
