using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoLoggerMessageGenerator.IntegrationTests;

public static class ComplexObjectExample
{
    private static readonly ILogger Logger = NullLogger.Instance;

    public static void Run()
    {
        var obj = new ComplexObject("1", new NestedObject("2", 3));
        Logger.LogInformation("Hello world {ComplexObject}!", obj);
    }
}

public record ComplexObject(string Id, NestedObject Value);

public record NestedObject (string Id2, int Value2);
