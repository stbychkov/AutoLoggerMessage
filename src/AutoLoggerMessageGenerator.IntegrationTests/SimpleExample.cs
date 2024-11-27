using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace AutoLoggerMessageGenerator.IntegrationTests;

public static class Program
{
    private static readonly ILogger Logger = new ConsoleLogger("console", (category, level) => true, true);

    public static void Main()
    {
        Logger.LogWarning("Hello world {arg1} {arg2}!", 1, true);
    }
}
