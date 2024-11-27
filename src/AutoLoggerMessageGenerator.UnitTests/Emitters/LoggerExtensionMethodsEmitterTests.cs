using FluentAssertions;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

public class LoggerExtensionMethodsEmitterTests
{
    [Fact]
    public void LoggerExtensionsMustBeAvailableForReference()
    {
        typeof(AutoLoggerMessageGenerator.Emitters.LoggerExtensions).Should().NotBeNull();
    }
}
