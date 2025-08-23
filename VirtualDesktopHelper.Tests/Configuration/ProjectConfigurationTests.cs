using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using VirtualDesktopHelper.Configuration;
using Xunit;

namespace VirtualDesktopHelper.Tests.Configuration
{
    public class ProjectConfigurationTests : IDisposable
    {
        private readonly string _testConfigPath;
        private readonly string _testDirectory;

        public ProjectConfigurationTests()
        {
            // Use a test-specific directory to avoid conflicts
            _testDirectory = Path.Combine(Path.GetTempPath(), "VirtualDesktopTests");
            _testConfigPath = Path.Combine(_testDirectory, "project_config.json");
            
            // Clean up any existing test files
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
            
            // Reset the singleton instance before each test
            ProjectConfiguration.Reset();
        }

        public void Dispose()
        {
            // Clean up test files
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
            ProjectConfiguration.Reset();
        }

        [Fact]
        public void DetectProject_ShouldReturnDefaultProject_WhenDesktopNameIsNull()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;

            // Act
            var result = config.DetectProject(null!);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3572980);
            result.Name.Should().Be("Afwezig");
        }

        [Fact]
        public void DetectProject_ShouldReturnDefaultProject_WhenDesktopNameIsEmpty()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;

            // Act
            var result = config.DetectProject("");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3572980);
            result.Name.Should().Be("Afwezig");
        }

        [Fact]
        public void DetectProject_ShouldReturnDefaultProject_WhenDesktopNameIsWhitespace()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;

            // Act
            var result = config.DetectProject("   ");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3572980);
            result.Name.Should().Be("Afwezig");
        }

        [Theory]
        [InlineData("Simon Desktop", "DWH - Technisch Onderhoud Selene")]
        [InlineData("selene work", "DWH - Technisch Onderhoud Selene")]
        [InlineData("Docker Development", "DWH - AI/ML Technische Verbeteringen")]
        [InlineData("AZUREML Tasks", "DWH - AI/ML Technische Verbeteringen")]
        [InlineData("archief management", "DWH - Data Anonimisatie en Archivering")]
        public void DetectProject_ShouldReturnCorrectProject_WhenKeywordMatches(string desktopName, string expectedProjectName)
        {
            // Arrange
            var config = ProjectConfiguration.Instance;

            // Act
            var result = config.DetectProject(desktopName);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(expectedProjectName);
        }

        [Theory]
        [InlineData("Random Desktop")]
        [InlineData("No Match Here")]
        [InlineData("Unknown Project")]
        public void DetectProject_ShouldReturnDefaultProject_WhenNoKeywordMatches(string desktopName)
        {
            // Arrange
            var config = ProjectConfiguration.Instance;

            // Act
            var result = config.DetectProject(desktopName);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3572980);
            result.Name.Should().Be("Afwezig");
        }

        [Fact]
        public void DetectProject_ShouldBeCaseInsensitive()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;

            // Act
            var resultLowercase = config.DetectProject("simon");
            var resultUppercase = config.DetectProject("SIMON");
            var resultMixedCase = config.DetectProject("SiMoN");

            // Assert
            resultLowercase.Name.Should().Be("DWH - Technisch Onderhoud Selene");
            resultUppercase.Name.Should().Be("DWH - Technisch Onderhoud Selene");
            resultMixedCase.Name.Should().Be("DWH - Technisch Onderhoud Selene");
        }

        [Fact]
        public void AddProjectMapping_ShouldAddNewMapping()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;
            var initialCount = config.ProjectMappings.Count;
            var keywords = new List<string> { "test", "unittest" };

            // Act
            config.AddProjectMapping(999, "Test Project", keywords);

            // Assert
            config.ProjectMappings.Should().HaveCount(initialCount + 1);
            var addedMapping = config.ProjectMappings.FirstOrDefault(m => m.Project.Id == 999);
            addedMapping.Should().NotBeNull();
            addedMapping!.Project.Name.Should().Be("Test Project");
            addedMapping.Keywords.Should().BeEquivalentTo(keywords);
        }

        [Fact]
        public void RemoveProjectMapping_ShouldRemoveExistingMapping()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;
            config.AddProjectMapping(999, "Test Project", new List<string> { "test" });
            var initialCount = config.ProjectMappings.Count;

            // Act
            config.RemoveProjectMapping(999);

            // Assert
            config.ProjectMappings.Should().HaveCount(initialCount - 1);
            config.ProjectMappings.Should().NotContain(m => m.Project.Id == 999);
        }

        [Fact]
        public void UpdateProjectMapping_ShouldUpdateExistingMapping()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;
            config.AddProjectMapping(999, "Test Project", new List<string> { "test" });
            var newKeywords = new List<string> { "updated", "test", "keywords" };

            // Act
            config.UpdateProjectMapping(999, "Updated Test Project", newKeywords);

            // Assert
            var updatedMapping = config.ProjectMappings.FirstOrDefault(m => m.Project.Id == 999);
            updatedMapping.Should().NotBeNull();
            updatedMapping!.Project.Name.Should().Be("Updated Test Project");
            updatedMapping.Keywords.Should().BeEquivalentTo(newKeywords);
        }

        [Fact]
        public void UpdateProjectMapping_ShouldDoNothing_WhenProjectIdNotFound()
        {
            // Arrange
            var config = ProjectConfiguration.Instance;
            var initialMappings = config.ProjectMappings.ToList();

            // Act
            config.UpdateProjectMapping(999, "Non-existent Project", new List<string> { "test" });

            // Assert
            config.ProjectMappings.Should().BeEquivalentTo(initialMappings);
        }

        [Fact]
        public void DefaultProject_ShouldHaveCorrectValues()
        {
            // Arrange & Act
            var config = ProjectConfiguration.Instance;

            // Assert
            config.DefaultProject.Should().NotBeNull();
            config.DefaultProject.Id.Should().Be(3572980);
            config.DefaultProject.Name.Should().Be("Afwezig");
        }

        [Fact]
        public void ProjectMappings_ShouldHaveDefaultMappings()
        {
            // Arrange & Act
            var config = ProjectConfiguration.Instance;

            // Assert
            config.ProjectMappings.Should().NotBeEmpty();
            config.ProjectMappings.Should().Contain(m => m.Keywords.Contains("simon"));
            config.ProjectMappings.Should().Contain(m => m.Keywords.Contains("docker"));
            config.ProjectMappings.Should().Contain(m => m.Keywords.Contains("archief"));
        }
    }
}
