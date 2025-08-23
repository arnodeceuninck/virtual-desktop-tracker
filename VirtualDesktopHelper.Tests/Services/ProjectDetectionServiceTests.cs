using FluentAssertions;
using Moq;
using System.Collections.Generic;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Models;
using VirtualDesktopHelper.Services;
using Xunit;

namespace VirtualDesktopHelper.Tests.Services
{
    public class ProjectDetectionServiceTests
    {
        private readonly ProjectDetectionService _service;

        public ProjectDetectionServiceTests()
        {
            // Use default project configuration since we can't easily mock the singleton
            _service = new ProjectDetectionService();
        }

        [Fact]
        public void DetectProjectForEntry_ShouldReturnCorrectProject()
        {
            // Arrange
            var entry = new DesktopUsageEntry { DesktopName = "simon desktop" };

            // Act
            var result = _service.DetectProjectForEntry(entry);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("DWH - Technisch Onderhoud Selene");
        }

        [Fact]
        public void DetectProjectsForEntries_ShouldReturnDictionaryWithAllEntries()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>
            {
                new() { DesktopName = "simon workspace" },
                new() { DesktopName = "docker development" },
                new() { DesktopName = "random desktop" }
            };

            // Act
            var result = _service.DetectProjectsForEntries(entries);

            // Assert
            result.Should().HaveCount(3);
            result[entries[0]].Name.Should().Be("DWH - Technisch Onderhoud Selene");
            result[entries[1]].Name.Should().Be("DWH - AI/ML Technische Verbeteringen");
            result[entries[2]].Name.Should().Be("Afwezig");
        }

        [Fact]
        public void DetectProjectsForEntries_ShouldHandleEmptyList()
        {
            // Arrange
            var entries = new List<DesktopUsageEntry>();

            // Act
            var result = _service.DetectProjectsForEntries(entries);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("simon development")]
        [InlineData("docker testing")]
        [InlineData("archief production")]
        public void DetectProjectForEntry_ShouldHandleDifferentDesktopNames(string desktopName)
        {
            // Arrange
            var entry = new DesktopUsageEntry { DesktopName = desktopName };

            // Act
            var result = _service.DetectProjectForEntry(entry);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().NotBeNullOrEmpty();
        }
    }
}
