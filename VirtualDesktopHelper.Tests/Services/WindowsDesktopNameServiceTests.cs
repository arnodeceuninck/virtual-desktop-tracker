using FluentAssertions;
using Moq;
using VirtualDesktopHelper.Configuration;
using VirtualDesktopHelper.Interfaces;
using VirtualDesktopHelper.Services;
using Xunit;

namespace VirtualDesktopHelper.Tests.Services
{
    public class WindowsDesktopNameServiceTests
    {
        private readonly Mock<IScreenStateDetector> _mockScreenStateDetector;
        private readonly Mock<IVirtualDesktopErrorHandler> _mockErrorHandler;
        private readonly TrackerConfiguration _testConfig;
        private readonly WindowsDesktopNameService _service;

        public WindowsDesktopNameServiceTests()
        {
            _mockScreenStateDetector = new Mock<IScreenStateDetector>();
            _mockErrorHandler = new Mock<IVirtualDesktopErrorHandler>();
            _testConfig = new TrackerConfiguration();
            
            _service = new WindowsDesktopNameService(
                _mockScreenStateDetector.Object,
                _mockErrorHandler.Object,
                _testConfig);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenScreenStateDetectorIsNull()
        {
            // Act & Assert
            var action = () => new WindowsDesktopNameService(
                null!,
                _mockErrorHandler.Object,
                _testConfig);

            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("screenStateDetector");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenErrorHandlerIsNull()
        {
            // Act & Assert
            var action = () => new WindowsDesktopNameService(
                _mockScreenStateDetector.Object,
                null!,
                _testConfig);

            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("errorHandler");
        }

        [Fact]
        public void GetCurrentDesktopName_ShouldReturnScreenOff_WhenScreenIsLockedOrOff()
        {
            // Arrange
            _mockScreenStateDetector.Setup(x => x.IsScreenLockedOrOff()).Returns(true);
            _mockErrorHandler.Setup(x => x.ExecuteWithRetry(
                It.IsAny<Func<string>>(),
                "GetCurrentDesktopName",
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
                .Returns<Func<string>, string, int, TimeSpan>((func, _, _, _) => func());

            // Act
            var result = _service.GetCurrentDesktopName();

            // Assert
            result.Should().Be("Screen Off");
            _mockScreenStateDetector.Verify(x => x.IsScreenLockedOrOff(), Times.Once);
        }

        [Fact]
        public void GetCurrentDesktopName_ShouldCallErrorHandlerExecuteWithRetry()
        {
            // Arrange
            _mockScreenStateDetector.Setup(x => x.IsScreenLockedOrOff()).Returns(false);
            _mockErrorHandler.Setup(x => x.ExecuteWithRetry(
                It.IsAny<Func<string>>(),
                "GetCurrentDesktopName",
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
                .Returns("Desktop 1");

            // Act
            var result = _service.GetCurrentDesktopName();

            // Assert
            result.Should().Be("Desktop 1");
            _mockErrorHandler.Verify(x => x.ExecuteWithRetry(
                It.IsAny<Func<string>>(),
                "GetCurrentDesktopName",
                _testConfig.SubprocessRetryCount,
                _testConfig.SubprocessRetryDelay), 
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void RenameCurrentDesktop_ShouldReturnFalse_WhenNameIsNullOrWhitespace(string? desktopName)
        {
            // Act
            var result = _service.RenameCurrentDesktop(desktopName!);

            // Assert
            result.Should().BeFalse();
            _mockErrorHandler.Verify(x => x.LogWarning(
                "Empty or null desktop name provided", 
                "RenameCurrentDesktop"), 
                Times.Once);
        }

        [Fact]
        public void RenameCurrentDesktop_ShouldReturnTrue_WhenNameIsValid()
        {
            // Arrange
            var newName = "New Desktop Name";
            _mockErrorHandler.Setup(x => x.ExecuteWithRetry(
                It.IsAny<Func<bool>>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>()))
                .Returns(true);

            // Act
            var result = _service.RenameCurrentDesktop(newName);

            // Assert
            result.Should().BeTrue();
        }
    }
}
