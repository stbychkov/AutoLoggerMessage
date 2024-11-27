using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoLoggerMessageGenerator.IntegrationTests;

public static class SimpleExample
{
    private static readonly ILogger Logger = NullLogger.Instance;

    public static void Run()
    {
        Logger.LogWarning("Hello world {arg1} {arg2}!", 1, true);
    }
}
