using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.IntegrationTests;

internal class LogPropertiesAttributeTests
{
    private static readonly ILogger Logger = LoggerFactory.Create(c =>
        c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
    ).CreateLogger<LogPropertiesAttributeTests>();

    [Test]
    public async Task AllLogPropertiesHaveToBeLogged()
    {
        IEvent generatorCallCapturedEvent = new GeneratorCallCapturedEvent { Id = Guid.NewGuid() };
        var proxy = DispatchProxyExecutionVerificationDecorator<IEvent>.Decorate(
            generatorCallCapturedEvent, Constants.LoggerMessageGeneratorName
        );
        generatorCallCapturedEvent = (IEvent) proxy;

        var propertiesCount = typeof(GeneratorCallCapturedEvent)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Length;

        Logger.LogInformation("Event received: {event}", generatorCallCapturedEvent);

        await Assert.That(proxy.ExecutionsWithoutGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsFromGenerator.Count).IsEqualTo(propertiesCount);
    }
}

internal interface IEvent
{
    Guid Id { get; }

    string Name { get; }

    long Timestamp { get; }
}

internal sealed record GeneratorCallCapturedEvent : IEvent
{
    public required Guid Id { get; init; }

    public string Name => nameof(GeneratorCallCapturedEvent);

    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
