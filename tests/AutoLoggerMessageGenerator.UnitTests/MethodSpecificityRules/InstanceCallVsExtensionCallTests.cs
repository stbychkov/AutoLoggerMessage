namespace AutoLoggerMessageGenerator.UnitTests;

internal class InstanceCallVsExtensionCallTests
{
    [Test]
    public async Task BeginScope_WithOnlyMessageParameter_InstanceCallShouldBePrioritized()
    {
        ILogger logger = new Logger();
        var result = logger.BeginScope("Some scope");

        await Assert.That(result).IsEqualTo(CallSource.Instance);
    }
}

interface ILogger
{
    CallSource BeginScope<T>(T _);
}
class Logger : ILogger
{
    public CallSource BeginScope<T>(T _) => CallSource.Instance;
}

static class LoggerExtensions
{
    public static CallSource BeginScope(this ILogger _, string __) => CallSource.Extension;
}

enum CallSource { Instance, Extension }
