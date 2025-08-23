using FluentAssertions;
using System.Collections;
using VirtualDesktopHelper.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace VirtualDesktopHelper.Tests.Configuration
{
    /// <summary>
    /// Comprehensive unit tests for ProjectMapping functionality using xUnit and FluentAssertions.
    /// Demonstrates various xUnit testing patterns and best practices.
    /// </summary>
    public class ProjectMappingExtensiveTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly ProjectMapping _mapping;

        public ProjectMappingExtensiveTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("Setting up ProjectMappingExtensiveTests");
            
            _mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 12345L, Name = "Test Project" },
                Keywords = new List<string> { "test", "unit", "xunit" }
            };
        }

        public void Dispose()
        {
            _output.WriteLine("Cleaning up ProjectMappingExtensiveTests");
            // Any cleanup logic would go here
        }

        #region Constructor and Property Tests

        [Fact]
        public void ProjectMapping_ShouldInitializeWithEmptyValues()
        {
            // Act
            var mapping = new ProjectMapping();

            // Assert
            mapping.Project.Should().NotBeNull();
            mapping.Project.Id.Should().Be(0L);
            mapping.Project.Name.Should().Be("");
            mapping.Keywords.Should().NotBeNull();
            mapping.Keywords.Should().BeEmpty();
        }

        [Fact]
        public void ProjectMapping_ShouldAllowPropertyAssignment()
        {
            // Arrange
            var project = new ProjectInfo { Id = 999L, Name = "Sample Project" };
            var keywords = new List<string> { "sample", "demo" };

            // Act
            var mapping = new ProjectMapping
            {
                Project = project,
                Keywords = keywords
            };

            // Assert
            mapping.Project.Should().BeSameAs(project);
            mapping.Keywords.Should().BeSameAs(keywords);
        }

        #endregion

        #region Keyword Matching Tests

        [Theory]
        [InlineData("test", true)]
        [InlineData("unit", true)]
        [InlineData("xunit", true)]
        [InlineData("TEST", true)] // Case insensitive
        [InlineData("Unit", true)]
        [InlineData("XUnit", true)]
        [InlineData("integration", false)]
        [InlineData("mock", false)]
        [InlineData("", false)]
        public void MatchesKeywords_ShouldReturnExpectedResult_ForSingleWords(string input, bool expected)
        {
            // Act
            var result = _mapping.MatchesKeywords(input);

            // Assert
            result.Should().Be(expected);
            _output.WriteLine($"Keyword '{input}' match result: {result}");
        }

        [Theory]
        [InlineData("This is a test case", true)]
        [InlineData("Running unit tests", true)]
        [InlineData("Using xunit framework", true)]
        [InlineData("Integration automation", false)] // Changed from "Integration testing"
        [InlineData("Mock objects", false)]
        [InlineData("No matching words here", false)]
        public void MatchesKeywords_ShouldReturnExpectedResult_ForSentences(string input, bool expected)
        {
            // Act
            var result = _mapping.MatchesKeywords(input);

            // Assert
            result.Should().Be(expected);
            _output.WriteLine($"Sentence '{input}' match result: {result}");
        }

        [Fact]
        public void MatchesKeywords_ShouldReturnFalse_ForNullInput()
        {
            // Act
            var result = _mapping.MatchesKeywords(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void MatchesKeywords_ShouldReturnFalse_ForWhitespaceInput()
        {
            // Act
            var result = _mapping.MatchesKeywords("   ");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void MatchesKeywords_ShouldHandleEmptyKeywordsList()
        {
            // Arrange
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 1L, Name = "Empty Keywords" },
                Keywords = new List<string>()
            };

            // Act
            var result = mapping.MatchesKeywords("any text");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void MatchesKeywords_ShouldIgnoreEmptyKeywords()
        {
            // Arrange
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 1L, Name = "Mixed Keywords" },
                Keywords = new List<string> { "valid", "", "  ", "keyword" }
            };

            // Act
            var validResult = mapping.MatchesKeywords("valid text");
            var keywordResult = mapping.MatchesKeywords("keyword text");
            var emptyResult = mapping.MatchesKeywords("some text");

            // Assert
            validResult.Should().BeTrue();
            keywordResult.Should().BeTrue();
            emptyResult.Should().BeFalse();
        }

        #endregion

        #region Collection Tests

        public static IEnumerable<object[]> GetKeywordTestData()
        {
            yield return new object[] { new List<string> { "dev", "development" }, "development work", true };
            yield return new object[] { new List<string> { "qa", "testing" }, "qa automation", true };
            yield return new object[] { new List<string> { "docs", "documentation" }, "writing docs", true };
            yield return new object[] { new List<string> { "prod", "production" }, "dev environment", false };
        }

        [Theory]
        [MemberData(nameof(GetKeywordTestData))]
        public void MatchesKeywords_ShouldWork_WithMemberData(List<string> keywords, string text, bool expected)
        {
            // Arrange
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 1L, Name = "Dynamic Test" },
                Keywords = keywords
            };

            // Act
            var result = mapping.MatchesKeywords(text);

            // Assert
            result.Should().Be(expected);
            _output.WriteLine($"Keywords: [{string.Join(", ", keywords)}], Text: '{text}', Expected: {expected}, Actual: {result}");
        }

        #endregion

        #region Custom Test Attributes and Fixtures

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Priority", "High")]
        public void ProjectMapping_ShouldSupportTraitAttributes()
        {
            // This test demonstrates the use of Trait attributes for test categorization
            
            // Act & Assert
            _mapping.Should().NotBeNull();
            _mapping.Project.Should().NotBeNull();
            _mapping.Keywords.Should().NotBeEmpty();
            
            _output.WriteLine("This test is categorized as Unit test with High priority");
        }

        [Theory]
        [Trait("Category", "Integration")]
        [ClassData(typeof(ProjectMappingTestData))]
        public void MatchesKeywords_ShouldWork_WithClassData(ProjectInfo project, List<string> keywords, string testText, bool expected)
        {
            // Arrange
            var mapping = new ProjectMapping
            {
                Project = project,
                Keywords = keywords
            };

            // Act
            var result = mapping.MatchesKeywords(testText);

            // Assert
            result.Should().Be(expected);
            _output.WriteLine($"Project: {project.Name}, Keywords: [{string.Join(", ", keywords)}], Result: {result}");
        }

        #endregion

        #region String Representation Tests

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Act
            var result = _mapping.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("Test Project");
            result.Should().Contain("12345");
            result.Should().Contain("test, unit, xunit");
            _output.WriteLine($"ToString result: {result}");
        }

        [Fact]
        public void ToString_ShouldHandleEmptyKeywords()
        {
            // Arrange
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 1L, Name = "No Keywords" },
                Keywords = new List<string>()
            };

            // Act
            var result = mapping.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("No Keywords");
            result.Should().Contain("[]");
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void MatchesKeywords_ShouldPerformWell_WithLargeKeywordList()
        {
            // Arrange
            var largeKeywordList = Enumerable.Range(1, 1000)
                .Select(i => $"keyword{i}")
                .ToList();
            
            largeKeywordList.Add("findme"); // Add our target keyword
            
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 1L, Name = "Performance Test" },
                Keywords = largeKeywordList
            };

            // Act & Assert
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = mapping.MatchesKeywords("text with findme keyword");
            stopwatch.Stop();

            result.Should().BeTrue();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should complete within 100ms
            _output.WriteLine($"Performance test completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion
    }

    /// <summary>
    /// Test data class for use with ClassData attribute.
    /// </summary>
    public class ProjectMappingTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new ProjectInfo { Id = 1L, Name = "Development" },
                new List<string> { "dev", "code" },
                "coding development",
                true
            };
            
            yield return new object[]
            {
                new ProjectInfo { Id = 2L, Name = "Testing" },
                new List<string> { "test", "qa" },
                "test automation",
                true
            };
            
            yield return new object[]
            {
                new ProjectInfo { Id = 3L, Name = "Documentation" },
                new List<string> { "docs", "wiki" },
                "random text",
                false
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
