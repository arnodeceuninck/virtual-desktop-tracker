using FluentAssertions;
using VirtualDesktopHelper.Configuration;
using Xunit;

namespace VirtualDesktopHelper.Tests.Models
{
    public class ProjectInfoTests
    {
        [Fact]
        public void ProjectInfo_ShouldHaveSettableProperties()
        {
            // Arrange & Act
            var projectInfo = new ProjectInfo
            {
                Id = 12345L,
                Name = "Test Project"
            };

            // Assert
            projectInfo.Id.Should().Be(12345L);
            projectInfo.Name.Should().Be("Test Project");
        }

        [Fact]
        public void ProjectInfo_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var projectInfo = new ProjectInfo();

            // Assert
            projectInfo.Id.Should().Be(0L);
            projectInfo.Name.Should().Be("");
        }
    }

    public class ProjectMappingTests
    {
        [Fact]
        public void ProjectMapping_ShouldHaveSettableProperties()
        {
            // Arrange & Act
            var mapping = new ProjectMapping
            {
                Project = new ProjectInfo { Id = 123L, Name = "Test" },
                Keywords = new List<string> { "keyword1", "keyword2" }
            };

            // Assert
            mapping.Project.Should().NotBeNull();
            mapping.Project.Id.Should().Be(123L);
            mapping.Keywords.Should().HaveCount(2);
            mapping.Keywords.Should().Contain("keyword1");
            mapping.Keywords.Should().Contain("keyword2");
        }

        [Fact]
        public void ProjectMapping_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var mapping = new ProjectMapping();

            // Assert
            mapping.Project.Should().NotBeNull();
            mapping.Keywords.Should().NotBeNull();
            mapping.Keywords.Should().BeEmpty();
        }
    }
}
